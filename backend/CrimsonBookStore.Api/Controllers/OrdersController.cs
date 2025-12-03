using CrimsonBookStore.Api.DTOs;
using CrimsonBookStore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrimsonBookStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _orderService;
    private readonly ICartService _cartService;

    public OrdersController(IPurchaseOrderService orderService, ICartService cartService)
    {
        _orderService = orderService;
        _cartService = cartService;
    }

    private string GetSessionId()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("SessionId")))
        {
            HttpContext.Session.SetString("SessionId", Guid.NewGuid().ToString());
        }
        return HttpContext.Session.GetString("SessionId")!;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        var sessionId = GetSessionId();
        
        try
        {
            var order = await _orderService.CheckoutAsync(request.UserID, sessionId);
            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("customer/{userId}")]
    public async Task<IActionResult> GetCustomerOrders(int userId)
    {
        var orders = await _orderService.GetOrdersByUserIdAsync(userId);
        return Ok(orders);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        // TODO: Add admin authorization check
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        // TODO: Add admin authorization check
        var success = await _orderService.UpdateOrderStatusAsync(id, request);
        if (!success)
        {
            return NotFound();
        }
        return Ok(new { message = "Order status updated" });
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var success = await _orderService.CancelOrderAsync(id);
        if (!success)
        {
            return BadRequest(new { message = "Order cannot be cancelled" });
        }
        return Ok(new { message = "Order cancelled and stock restored" });
    }

    [HttpGet("customer/{userId}/purchased-books")]
    public async Task<IActionResult> GetPurchasedBooks(int userId)
    {
        var orders = await _orderService.GetOrdersByUserIdAsync(userId);
        var purchasedBooks = new List<object>();
        
        foreach (var order in orders)
        {
            if (order.Status != "Cancelled")
            {
                foreach (var lineItem in order.LineItems)
                {
                    purchasedBooks.Add(new
                    {
                        bookID = lineItem.BookID,
                        title = lineItem.BookTitle,
                        quantity = lineItem.Quantity,
                        purchaseDate = order.OrderDate,
                        orderID = order.POID
                    });
                }
            }
        }
        
        return Ok(purchasedBooks);
    }
}

