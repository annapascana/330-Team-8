using CrimsonBookStore.Api.Data;
using CrimsonBookStore.Api.Models;
using Dapper;

namespace CrimsonBookStore.Api.Repositories;

public class CartRepository : ICartRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private static readonly Dictionary<string, List<CartItem>> _sessionCarts = new();

    public CartRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public Task<List<CartItem>> GetCartItemsAsync(string sessionId)
    {
        if (!_sessionCarts.ContainsKey(sessionId))
        {
            _sessionCarts[sessionId] = new List<CartItem>();
        }
        return Task.FromResult(_sessionCarts[sessionId]);
    }

    public async Task<bool> AddItemAsync(string sessionId, CartItem item)
    {
        var cart = await GetCartItemsAsync(sessionId);
        var existingItem = cart.FirstOrDefault(x => x.BookID == item.BookID);
        
        if (existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            // Load book details with author
            using var conn = _connectionFactory.CreateConnection();
            var sql = @"SELECT b.*, GROUP_CONCAT(a.AuthName SEPARATOR ', ') AS Author
                        FROM Book b
                        LEFT JOIN AuthoredBy ab ON b.BookID = ab.BookID
                        LEFT JOIN Author a ON ab.AuthID = a.AuthID
                        WHERE b.BookID = @BookID
                        GROUP BY b.BookID";
            var book = await conn.QueryFirstOrDefaultAsync<Book>(sql, new { BookID = item.BookID });
            item.Book = book;
            cart.Add(item);
        }
        return true;
    }

    public Task<bool> UpdateItemAsync(string sessionId, int bookId, int quantity)
    {
        if (!_sessionCarts.ContainsKey(sessionId))
        {
            return Task.FromResult(false);
        }
        
        var cart = _sessionCarts[sessionId];
        var item = cart.FirstOrDefault(x => x.BookID == bookId);
        
        if (item != null)
        {
            if (quantity <= 0)
            {
                cart.Remove(item);
            }
            else
            {
                item.Quantity = quantity;
            }
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> RemoveItemAsync(string sessionId, int bookId)
    {
        if (!_sessionCarts.ContainsKey(sessionId))
        {
            return Task.FromResult(false);
        }
        
        var cart = _sessionCarts[sessionId];
        var item = cart.FirstOrDefault(x => x.BookID == bookId);
        if (item != null)
        {
            cart.Remove(item);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> ClearCartAsync(string sessionId)
    {
        if (_sessionCarts.ContainsKey(sessionId))
        {
            _sessionCarts[sessionId].Clear();
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}

