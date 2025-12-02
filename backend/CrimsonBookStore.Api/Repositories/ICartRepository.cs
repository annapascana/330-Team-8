using CrimsonBookStore.Api.Models;

namespace CrimsonBookStore.Api.Repositories;

public interface ICartRepository
{
    Task<List<CartItem>> GetCartItemsAsync(string sessionId);
    Task<bool> AddItemAsync(string sessionId, CartItem item);
    Task<bool> UpdateItemAsync(string sessionId, int bookId, int quantity);
    Task<bool> RemoveItemAsync(string sessionId, int bookId);
    Task<bool> ClearCartAsync(string sessionId);
}

