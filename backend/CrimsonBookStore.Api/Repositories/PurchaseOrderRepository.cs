using CrimsonBookStore.Api.Data;
using CrimsonBookStore.Api.Models;
using Dapper;

namespace CrimsonBookStore.Api.Repositories;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PurchaseOrderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<PurchaseOrder>> GetByUserIdAsync(int userId)
    {
        using var conn = _connectionFactory.CreateConnection();
        var orders = await conn.QueryAsync<PurchaseOrder>(
            "SELECT * FROM PurchaseOrder WHERE UserID = @UserID AND POID > 0 ORDER BY UpdAt DESC",
            new { UserID = userId });
        
        var ordersList = orders.ToList();
        foreach (var order in ordersList)
        {
            try
            {
                order.LineItems = await GetLineItemsByOrderIdAsync(order.POID);
            }
            catch
            {
                // If line items fail to load, set empty list but still return the order
                order.LineItems = new List<OrderLineItem>();
            }
        }
        return ordersList;
    }

    public async Task<List<PurchaseOrder>> GetAllAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        var orders = await conn.QueryAsync<PurchaseOrder>(
            "SELECT * FROM PurchaseOrder ORDER BY UpdAt DESC");
        
        var ordersList = orders.ToList();
        foreach (var order in ordersList)
        {
            order.LineItems = await GetLineItemsByOrderIdAsync(order.POID);
        }
        return ordersList;
    }

    public async Task<PurchaseOrder?> GetByIdAsync(int poId)
    {
        using var conn = _connectionFactory.CreateConnection();
        var order = await conn.QueryFirstOrDefaultAsync<PurchaseOrder>(
            "SELECT * FROM PurchaseOrder WHERE POID = @POID",
            new { POID = poId });
        
        if (order != null)
        {
            order.LineItems = await GetLineItemsByOrderIdAsync(poId);
        }
        return order;
    }

    public async Task<int> CreateOrderAsync(PurchaseOrder order)
    {
        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync();
        
        using var transaction = await conn.BeginTransactionAsync();
        
        try
        {
            // Clean up any orphaned records with POID = 0 (shouldn't exist, but handle edge case)
            // Must delete child records (POItem) first due to foreign key constraint
            await conn.ExecuteAsync("DELETE FROM POItem WHERE POID = 0", transaction: transaction);
            await conn.ExecuteAsync("DELETE FROM PurchaseOrder WHERE POID = 0", transaction: transaction);
            
            // Get the next POID (since POID is not AUTO_INCREMENT, we need to generate it manually)
            var maxId = await conn.QuerySingleAsync<int>("SELECT COALESCE(MAX(POID), 0) FROM PurchaseOrder", transaction: transaction);
            var orderId = maxId + 1;
            
            // Note: BookID in PurchaseOrder is from ERD, but we'll use first book from line items
            var sql = @"INSERT INTO PurchaseOrder (POID, UserID, BookID, Status, SubTot, Tax, Total, UpdAt)
                        VALUES (@POID, @UserID, @BookID, @Status, @SubTot, @Tax, @Total, @UpdAt)";
            
            var parameters = new { 
                POID = orderId,
                UserID = order.UserID, 
                BookID = order.BookID, 
                Status = order.Status, 
                SubTot = order.SubTot, 
                Tax = order.Tax, 
                Total = order.Total, 
                UpdAt = order.UpdAt 
            };
            
            // Execute INSERT
            var rowsAffected = await conn.ExecuteAsync(sql, parameters, transaction: transaction);
            
            if (rowsAffected == 0)
            {
                await transaction.RollbackAsync();
                throw new Exception("Failed to create purchase order");
            }
            
            await transaction.CommitAsync();
            return orderId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> CreateLineItemAsync(OrderLineItem lineItem)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO POItem (POID, LineNo, BookID, Qty, UnitPrice, LineTot)
                    VALUES (@POID, @LineNo, @BookID, @Qty, @UnitPrice, @LineTot)";
        var rowsAffected = await conn.ExecuteAsync(sql, lineItem);
        return rowsAffected > 0;
    }

    public async Task<int> CreateOrderWithLineItemsAsync(PurchaseOrder order, List<OrderLineItem> lineItems)
    {
        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync();
        
        using var transaction = await conn.BeginTransactionAsync();
        
        try
        {
            // Clean up any orphaned records with POID = 0 (shouldn't exist, but handle edge case)
            // Must delete child records (POItem) first due to foreign key constraint
            await conn.ExecuteAsync("DELETE FROM POItem WHERE POID = 0", transaction: transaction);
            await conn.ExecuteAsync("DELETE FROM PurchaseOrder WHERE POID = 0", transaction: transaction);
            
            // Get the next POID (since POID is not AUTO_INCREMENT, we need to generate it manually)
            var maxId = await conn.QuerySingleAsync<int>("SELECT COALESCE(MAX(POID), 0) FROM PurchaseOrder", transaction: transaction);
            var orderId = maxId + 1;
            
            // Clean up any existing line items for this order ID (in case of retry or partial failure)
            // This prevents duplicate key errors if a previous attempt partially succeeded
            await conn.ExecuteAsync("DELETE FROM POItem WHERE POID = @POID", new { POID = orderId }, transaction: transaction);
            
            // Insert the order with explicit POID
            var sql = @"INSERT INTO PurchaseOrder (POID, UserID, BookID, Status, SubTot, Tax, Total, UpdAt)
                        VALUES (@POID, @UserID, @BookID, @Status, @SubTot, @Tax, @Total, @UpdAt)";
            
            var parameters = new { 
                POID = orderId,
                UserID = order.UserID, 
                BookID = order.BookID, 
                Status = order.Status, 
                SubTot = order.SubTot, 
                Tax = order.Tax, 
                Total = order.Total, 
                UpdAt = order.UpdAt 
            };
            
            var rowsAffected = await conn.ExecuteAsync(sql, parameters, transaction: transaction);
            
            if (rowsAffected == 0)
            {
                await transaction.RollbackAsync();
                throw new Exception("Failed to create purchase order");
            }
            
            // Create all line items and update stock in the same transaction
            int lineNo = 1;
            foreach (var lineItem in lineItems)
            {
                lineItem.POID = orderId;
                lineItem.LineNo = lineNo++;
                
                var lineItemSql = @"INSERT INTO POItem (POID, LineNo, BookID, Qty, UnitPrice, LineTot)
                                    VALUES (@POID, @LineNo, @BookID, @Qty, @UnitPrice, @LineTot)";
                var lineItemRowsAffected = await conn.ExecuteAsync(lineItemSql, lineItem, transaction: transaction);
                
                if (lineItemRowsAffected == 0)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"Failed to create line item for book {lineItem.BookID}");
                }
                
                // Update stock within the same transaction
                var stockUpdateSql = @"UPDATE Book SET StockQty = StockQty - @Quantity
                                       WHERE BookID = @BookID AND StockQty >= @Quantity";
                var stockRowsAffected = await conn.ExecuteAsync(stockUpdateSql, 
                    new { BookID = lineItem.BookID, Quantity = lineItem.Qty }, 
                    transaction: transaction);
                
                if (stockRowsAffected == 0)
                {
                    await transaction.RollbackAsync();
                    throw new Exception($"Insufficient stock for book {lineItem.BookID}");
                }
            }
            
            await transaction.CommitAsync();
            return orderId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(int poId, string status, DateTime? cancelledAt = null)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"UPDATE PurchaseOrder SET Status = @Status, UpdAt = NOW()
                    WHERE POID = @POID";
        var rowsAffected = await conn.ExecuteAsync(sql, new { POID = poId, Status = status });
        return rowsAffected > 0;
    }

    public async Task<List<OrderLineItem>> GetLineItemsByOrderIdAsync(int poId)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"SELECT poi.*, b.BookID, b.ISBN, b.Title, b.Edition, b.`Condition`, b.SellPrice,
                    GROUP_CONCAT(a.AuthName SEPARATOR ', ') AS Author
                    FROM POItem poi
                    INNER JOIN Book b ON poi.BookID = b.BookID
                    LEFT JOIN AuthoredBy ab ON b.BookID = ab.BookID
                    LEFT JOIN Author a ON ab.AuthID = a.AuthID
                    WHERE poi.POID = @POID
                    GROUP BY poi.POID, poi.LineNo, poi.BookID, poi.Qty, poi.UnitPrice, poi.LineTot, b.BookID, b.ISBN, b.Title, b.Edition, b.`Condition`, b.SellPrice";
        var items = await conn.QueryAsync<OrderLineItem, Book, OrderLineItem>(
            sql,
            (lineItem, book) =>
            {
                lineItem.Book = book;
                return lineItem;
            },
            new { POID = poId },
            splitOn: "BookID");
        return items.ToList();
    }

    public async Task<bool> DeleteOrderAsync(int poId)
    {
        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync();
        
        using var transaction = await conn.BeginTransactionAsync();
        
        try
        {
            // Delete child records first (POItem) due to foreign key constraint
            await conn.ExecuteAsync("DELETE FROM POItem WHERE POID = @POID", new { POID = poId }, transaction: transaction);
            
            // Then delete parent record (PurchaseOrder)
            var rowsAffected = await conn.ExecuteAsync("DELETE FROM PurchaseOrder WHERE POID = @POID", new { POID = poId }, transaction: transaction);
            
            await transaction.CommitAsync();
            return rowsAffected > 0;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteLineItemsByOrderIdAsync(int poId)
    {
        using var conn = _connectionFactory.CreateConnection();
        var rowsAffected = await conn.ExecuteAsync("DELETE FROM POItem WHERE POID = @POID", new { POID = poId });
        return rowsAffected >= 0; // Return true even if no rows deleted (idempotent)
    }
}

