using CrimsonBookStore.Api.DTOs;
using CrimsonBookStore.Api.Repositories;

namespace CrimsonBookStore.Api.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IPurchaseOrderRepository _orderRepository;
    private readonly ICartService _cartService;
    private readonly IBookRepository _bookRepository;

    public PurchaseOrderService(
        IPurchaseOrderRepository orderRepository,
        ICartService cartService,
        IBookRepository bookRepository)
    {
        _orderRepository = orderRepository;
        _cartService = cartService;
        _bookRepository = bookRepository;
    }

    public async Task<OrderResponse> CheckoutAsync(int userId, string sessionId)
    {
        var cart = await _cartService.GetCartAsync(sessionId);
        if (cart.Items.Count == 0)
        {
            throw new Exception("Cart is empty");
        }

        // Validate stock availability
        foreach (var item in cart.Items)
        {
            var book = await _bookRepository.GetByIdAsync(item.BookID);
            if (book == null || book.StockQty < item.Quantity)
            {
                throw new Exception($"Insufficient stock for book {item.BookID}");
            }
        }

        // Create order - use first book ID from cart (ERD has BookID in PurchaseOrder)
        var firstBookId = cart.Items.FirstOrDefault()?.BookID ?? 0;
        var order = new Api.Models.PurchaseOrder
        {
            UserID = userId,
            BookID = firstBookId,
            Status = "New",
            SubTot = cart.SubTotal,
            Tax = cart.Tax,
            Total = cart.Total,
            UpdAt = DateTime.UtcNow
        };

        var orderId = await _orderRepository.CreateOrderAsync(order);
        
        if (orderId <= 0)
        {
            throw new Exception("Failed to create order: Invalid order ID returned");
        }
        
        order.POID = orderId;

        try
        {
            // Clean up any orphaned line items from previous failed checkout attempts
            await _orderRepository.DeleteLineItemsByOrderIdAsync(orderId);

            // Create line items and decrement stock atomically
            int lineNo = 1;
            
            foreach (var item in cart.Items)
            {
                var lineItem = new Api.Models.OrderLineItem
                {
                    POID = orderId,
                    LineNo = lineNo++,
                    BookID = item.BookID,
                    Qty = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTot = item.LineTotal
                };

                await _orderRepository.CreateLineItemAsync(lineItem);
                
                // Decrement stock
                var success = await _bookRepository.UpdateStockAsync(item.BookID, -item.Quantity);
                if (!success)
                {
                    throw new Exception($"Failed to update stock for book {item.BookID}");
                }
            }

            // Clear cart
            await _cartService.ClearCartAsync(sessionId);

            // Reload the order from database to get line items with book details
            var completedOrder = await _orderRepository.GetByIdAsync(orderId);
            if (completedOrder == null)
            {
                throw new Exception($"Failed to retrieve created order with ID {orderId}");
            }

            return await MapToResponse(completedOrder);
        }
        catch
        {
            // If line item creation fails, clean up the orphaned order
            // Note: This is a best-effort cleanup - the order might still exist if cleanup fails
            try
            {
                await _orderRepository.DeleteOrderAsync(orderId);
            }
            catch
            {
                // Ignore cleanup errors
            }
            throw;
        }
    }

    public async Task<List<OrderResponse>> GetOrdersByUserIdAsync(int userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        var responses = new List<OrderResponse>();
        foreach (var order in orders)
        {
            try
            {
                // Filter out invalid orders (POID = 0) - these shouldn't exist but handle gracefully
                if (order.POID > 0)
                {
                    responses.Add(await MapToResponse(order));
                }
            }
            catch (Exception ex)
            {
                // Log error but continue processing other orders
                // This handles cases where line items might fail to load
                System.Diagnostics.Debug.WriteLine($"Error loading order {order.POID}: {ex.Message}");
            }
        }
        return responses;
    }

    public async Task<List<OrderResponse>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        var responses = new List<OrderResponse>();
        foreach (var order in orders)
        {
            // Filter out invalid orders (POID = 0) - repository already filters these
            if (order.POID > 0)
            {
                try
                {
                    responses.Add(await MapToResponse(order));
                }
                catch (Exception ex)
                {
                    // Log error but continue processing other orders
                    System.Diagnostics.Debug.WriteLine($"Error mapping order {order.POID}: {ex.Message}");
                    // Still add the order even if mapping fails partially
                    try
                    {
                        var response = new OrderResponse
                        {
                            POID = order.POID,
                            UserID = order.UserID,
                            OrderDate = order.UpdAt,
                            Status = order.Status,
                            SubTotal = order.SubTot,
                            Tax = order.Tax,
                            Total = order.Total,
                            CancelledAt = order.Status == "Cancelled" ? order.UpdAt : null,
                            LineItems = new List<OrderLineItemResponse>()
                        };
                        responses.Add(response);
                    }
                    catch
                    {
                        // Skip this order if we can't create even a basic response
                    }
                }
            }
        }
        return responses;
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request)
    {
        return await _orderRepository.UpdateStatusAsync(orderId, request.Status);
    }

    public async Task<bool> CancelOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null || order.Status == "Cancelled" || order.Status == "Completed")
        {
            return false;
        }

        // Restore stock
        foreach (var lineItem in order.LineItems)
        {
            await _bookRepository.UpdateStockAsync(lineItem.BookID, lineItem.Quantity);
        }

        return await _orderRepository.UpdateStatusAsync(orderId, "Cancelled", DateTime.UtcNow);
    }

    private Task<OrderResponse> MapToResponse(Api.Models.PurchaseOrder order)
    {
        var response = new OrderResponse
        {
            POID = order.POID,
            UserID = order.UserID,
            OrderDate = order.UpdAt,
            Status = order.Status,
            SubTotal = order.SubTot,
            Tax = order.Tax,
            Total = order.Total,
            CancelledAt = order.Status == "Cancelled" ? order.UpdAt : null
        };

        foreach (var lineItem in order.LineItems)
        {
            response.LineItems.Add(new OrderLineItemResponse
            {
                LineItemID = lineItem.LineItemID,
                BookID = lineItem.BookID,
                BookTitle = lineItem.Book?.Title ?? "Unknown",
                Quantity = lineItem.Quantity,
                UnitPrice = lineItem.UnitPrice,
                LineTotal = lineItem.LineTotal
            });
        }

        return Task.FromResult(response);
    }
}

