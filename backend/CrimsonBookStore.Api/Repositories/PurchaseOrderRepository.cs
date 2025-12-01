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
            "SELECT * FROM PurchaseOrder WHERE UserID = @UserID ORDER BY UpdAt DESC",
            new { UserID = userId });
        
        var ordersList = orders.ToList();
        foreach (var order in ordersList)
        {
            order.LineItems = await GetLineItemsByOrderIdAsync(order.POID);
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
        // Note: BookID in PurchaseOrder is from ERD, but we'll use first book from line items
        var sql = @"INSERT INTO PurchaseOrder (UserID, BookID, Status, SubTot, Tax, Total, UpdAt)
                    VALUES (@UserID, @BookID, @Status, @SubTot, @Tax, @Total, @UpdAt);
                    SELECT LAST_INSERT_ID();";
        return await conn.QuerySingleAsync<int>(sql, order);
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
        var sql = @"SELECT poi.*, b.BookID, b.Title, b.SellPrice,
                    GROUP_CONCAT(a.AuthName SEPARATOR ', ') AS Author
                    FROM POItem poi
                    INNER JOIN Book b ON poi.BookID = b.BookID
                    LEFT JOIN AuthoredBy ab ON b.BookID = ab.BookID
                    LEFT JOIN Author a ON ab.AuthID = a.AuthID
                    WHERE poi.POID = @POID
                    GROUP BY poi.POID, poi.LineNo, poi.BookID, poi.Qty, poi.UnitPrice, poi.LineTot, b.BookID, b.Title, b.SellPrice";
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
}

