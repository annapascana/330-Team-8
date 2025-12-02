using CrimsonBookStore.Api.DTOs;

namespace CrimsonBookStore.Api.Services;

public interface IPurchaseOrderService
{
    Task<OrderResponse> CheckoutAsync(int userId, string sessionId);
    Task<List<OrderResponse>> GetOrdersByUserIdAsync(int userId);
    Task<List<OrderResponse>> GetAllOrdersAsync();
    Task<bool> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request);
    Task<bool> CancelOrderAsync(int orderId);
}

