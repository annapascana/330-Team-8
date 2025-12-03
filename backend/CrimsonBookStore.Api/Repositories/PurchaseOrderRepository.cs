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
            "SELECT * FROM PurchaseOrder WHERE POID > 0 ORDER BY UpdAt DESC");
        
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
            
            // Note: BookID in PurchaseOrder is from ERD, but we'll use first book from line items
            var sql = @"INSERT INTO PurchaseOrder (UserID, BookID, Status, SubTot, Tax, Total, UpdAt)
                        VALUES (@UserID, @BookID, @Status, @SubTot, @Tax, @Total, @UpdAt)";
            
            // Explicitly create parameters without POID to prevent it from being included
            var parameters = new { 
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
            
            // Get the inserted ID
            var orderId = await conn.QuerySingleAsync<int>("SELECT LAST_INSERT_ID()", transaction: transaction);
            
            if (orderId == 0)
            {
                // If LAST_INSERT_ID() returns 0, try to get the max POID as fallback
                var maxId = await conn.QuerySingleAsync<int>("SELECT COALESCE(MAX(POID), 0) FROM PurchaseOrder", transaction: transaction);
                if (maxId > 0)
                {
                    orderId = maxId;
                }
                else
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Failed to retrieve purchase order ID. AUTO_INCREMENT may not be configured correctly.");
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

    public async Task<bool> CreateLineItemAsync(OrderLineItem lineItem)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO POItem (POID, LineNo, BookID, Qty, UnitPrice, LineTot)
                    VALUES (@POID, @LineNo, @BookID, @Qty, @UnitPrice, @LineTot)";
        var rowsAffected = await conn.ExecuteAsync(sql, lineItem);
        return rowsAffected > 0;
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
        
        // Query that joins with Book table to get book details
        // Note: Author comes from AuthoredBy junction table, we'll get it separately if needed
        var sql = @"SELECT poi.POID, poi.LineNo, poi.BookID, poi.Qty, poi.UnitPrice, poi.LineTot,
                    b.BookID, b.Title, b.ISBN, b.Edition, b.Condition, b.SellPrice, b.StockQty, b.Status,
                    GROUP_CONCAT(DISTINCT a.AuthName SEPARATOR ', ') as Author
                    FROM POItem poi
                    LEFT JOIN Book b ON poi.BookID = b.BookID
                    LEFT JOIN AuthoredBy ab ON b.BookID = ab.BookID
                    LEFT JOIN Author a ON ab.AuthID = a.AuthID
                    WHERE poi.POID = @POID
                    GROUP BY poi.POID, poi.LineNo, poi.BookID, poi.Qty, poi.UnitPrice, poi.LineTot,
                             b.BookID, b.Title, b.ISBN, b.Edition, b.Condition, b.SellPrice, b.StockQty, b.Status
                    ORDER BY poi.LineNo";
        
        var items = await conn.QueryAsync<OrderLineItem, Api.Models.Book, OrderLineItem>(
            sql,
            (lineItem, book) =>
            {
                lineItem.Book = book;
                // Author is already populated in the Book object from the GROUP_CONCAT
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

