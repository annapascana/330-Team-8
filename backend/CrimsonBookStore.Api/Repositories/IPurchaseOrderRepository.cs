using CrimsonBookStore.Api.Models;

namespace CrimsonBookStore.Api.Repositories;

public interface IPurchaseOrderRepository
{
    Task<List<PurchaseOrder>> GetByUserIdAsync(int userId);
    Task<List<PurchaseOrder>> GetAllAsync();
    Task<PurchaseOrder?> GetByIdAsync(int poId);
    Task<int> CreateOrderAsync(PurchaseOrder order);
    Task<bool> CreateLineItemAsync(OrderLineItem lineItem);
    Task<int> CreateOrderWithLineItemsAsync(PurchaseOrder order, List<OrderLineItem> lineItems);
    Task<bool> UpdateStatusAsync(int poId, string status, DateTime? cancelledAt = null);
    Task<List<OrderLineItem>> GetLineItemsByOrderIdAsync(int poId);
    Task<bool> DeleteOrderAsync(int poId);
    Task<bool> DeleteLineItemsByOrderIdAsync(int poId);
}

