using CrimsonBookStore.Api.DTOs;

namespace CrimsonBookStore.Api.Services;

public interface ICartService
{
    Task<CartResponse> GetCartAsync(string sessionId);
    Task<bool> AddToCartAsync(string sessionId, AddToCartRequest request);
    Task<bool> UpdateCartItemAsync(string sessionId, UpdateCartItemRequest request);
    Task<bool> RemoveFromCartAsync(string sessionId, int bookId);
    Task<bool> ClearCartAsync(string sessionId);
}

