using CrimsonBookStore.Api.Data;
using CrimsonBookStore.Api.Models;
using Dapper;

namespace CrimsonBookStore.Api.Repositories;

public class BookRepository : IBookRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BookRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Book>> GetAllAvailableAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        // Join with AuthoredBy to get author name
        var sql = @"SELECT b.*, GROUP_CONCAT(a.AuthName SEPARATOR ', ') AS Author
                    FROM Book b
                    LEFT JOIN AuthoredBy ab ON b.BookID = ab.BookID
                    LEFT JOIN Author a ON ab.AuthID = a.AuthID
                    WHERE b.Status = 'Available' AND b.StockQty > 0
                    GROUP BY b.BookID
                    ORDER BY b.Title";
        var books = await conn.QueryAsync<Book>(sql);
        return books.ToList();
    }

    public async Task<Book?> GetByIdAsync(int bookId)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"SELECT b.*, GROUP_CONCAT(a.AuthName SEPARATOR ', ') AS Author
                    FROM Book b
                    LEFT JOIN AuthoredBy ab ON b.BookID = ab.BookID
                    LEFT JOIN Author a ON ab.AuthID = a.AuthID
                    WHERE b.BookID = @BookID
                    GROUP BY b.BookID";
        return await conn.QueryFirstOrDefaultAsync<Book>(sql, new { BookID = bookId });
    }

    public async Task<List<Book>> SearchAsync(string? title, string? author, string? isbn, int? majorId, int? courseId)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"SELECT b.*, GROUP_CONCAT(a.AuthName SEPARATOR ', ') AS Author
                    FROM Book b
                    LEFT JOIN AuthoredBy ab ON b.BookID = ab.BookID
                    LEFT JOIN Author a ON ab.AuthID = a.AuthID
                    LEFT JOIN UsedIn ui ON b.BookID = ui.BookID
                    WHERE b.Status = 'Available' AND b.StockQty > 0";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(title))
        {
            sql += " AND b.Title LIKE @Title";
            parameters.Add("Title", $"%{title}%");
        }
        if (!string.IsNullOrWhiteSpace(author))
        {
            sql += " AND a.AuthName LIKE @Author";
            parameters.Add("Author", $"%{author}%");
        }
        if (!string.IsNullOrWhiteSpace(isbn))
        {
            sql += " AND b.ISBN = @ISBN";
            parameters.Add("ISBN", isbn);
        }
        if (majorId.HasValue)
        {
            sql += " AND ui.CourseID IN (SELECT CourseID FROM Course WHERE MajID = @MajorID)";
            parameters.Add("MajorID", majorId.Value);
        }
        if (courseId.HasValue)
        {
            sql += " AND ui.CourseID = @CourseID";
            parameters.Add("CourseID", courseId.Value);
        }

        sql += " GROUP BY b.BookID ORDER BY b.Title";
        var books = await conn.QueryAsync<Book>(sql, parameters);
        return books.ToList();
    }

    public async Task<int> CreateAsync(Book book)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO Book (ISBN, Title, Edition, Condition, AcqCost, 
                    SellPrice, StockQty, Status, SubID, CreatedAt)
                    VALUES (@ISBN, @Title, @Edition, @Condition, @AcqCost,
                    @SellPrice, @StockQty, @Status, @SubID, @CreatedAt);
                    SELECT LAST_INSERT_ID();";
        return await conn.QuerySingleAsync<int>(sql, book);
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"UPDATE Book SET Title = @Title, Edition = @Edition,
                    Condition = @Condition, AcqCost = @AcqCost,
                    SellPrice = @SellPrice, StockQty = @StockQty,
                    Status = @Status
                    WHERE BookID = @BookID";
        var rowsAffected = await conn.ExecuteAsync(sql, book);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int bookId)
    {
        using var conn = _connectionFactory.CreateConnection();
        var rowsAffected = await conn.ExecuteAsync(
            "DELETE FROM Book WHERE BookID = @BookID",
            new { BookID = bookId });
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateStockAsync(int bookId, int quantityChange)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"UPDATE Book SET StockQty = StockQty + @QuantityChange
                    WHERE BookID = @BookID AND StockQty + @QuantityChange >= 0";
        var rowsAffected = await conn.ExecuteAsync(sql, new { BookID = bookId, QuantityChange = quantityChange });
        return rowsAffected > 0;
    }

    public async Task<Book?> GetByISBNAsync(string isbn)
    {
        using var conn = _connectionFactory.CreateConnection();
        var sql = @"SELECT b.*, GROUP_CONCAT(a.AuthName SEPARATOR ', ') AS Author
                    FROM Book b
                    LEFT JOIN AuthoredBy ab ON b.BookID = ab.BookID
                    LEFT JOIN Author a ON ab.AuthID = a.AuthID
                    WHERE b.ISBN = @ISBN
                    GROUP BY b.BookID";
        return await conn.QueryFirstOrDefaultAsync<Book>(sql, new { ISBN = isbn });
    }
}

