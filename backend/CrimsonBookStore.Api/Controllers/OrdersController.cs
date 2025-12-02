using CrimsonBookStore.Api.DTOs;
using CrimsonBookStore.Api.Repositories;
using CrimsonBookStore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrimsonBookStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _orderService;
    private readonly ICartService _cartService;
    private readonly ISellSubmissionRepository _submissionRepository;

    public OrdersController(IPurchaseOrderService orderService, ICartService cartService, ISellSubmissionRepository submissionRepository)
    {
        _orderService = orderService;
        _cartService = cartService;
        _submissionRepository = submissionRepository;
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
        var purchasedBooksDict = new Dictionary<string, PurchasedBookInfo>();
        
        foreach (var order in orders)
        {
            if (order.Status != "Cancelled")
            {
                foreach (var lineItem in order.LineItems)
                {
                    var key = $"{lineItem.ISBN}_{lineItem.Edition}_{lineItem.Condition}";
                    
                    if (purchasedBooksDict.ContainsKey(key))
                    {
                        // Aggregate quantities for the same book
                        purchasedBooksDict[key].Quantity += lineItem.Quantity;
                    }
                    else
                    {
                        purchasedBooksDict[key] = new PurchasedBookInfo
                        {
                            BookID = lineItem.BookID,
                            Title = lineItem.BookTitle,
                            Author = lineItem.Author ?? "",
                            ISBN = lineItem.ISBN ?? "",
                            Edition = lineItem.Edition ?? "",
                            Condition = lineItem.Condition ?? "",
                            Quantity = lineItem.Quantity,
                            PurchaseDate = order.OrderDate,
                            OrderID = order.POID
                        };
                    }
                }
            }
        }
        
        // Subtract approved submissions from quantities
        var purchasedBooks = new List<object>();
        foreach (var kvp in purchasedBooksDict)
        {
            var book = kvp.Value;
            
            // Count how many approved submissions exist for this user and ISBN
            var soldCount = await _submissionRepository.CountApprovedByUserAndISBNAsync(userId, book.ISBN);
            var availableQuantity = Math.Max(0, book.Quantity - soldCount);
            
            // Only include books that still have available quantity
            if (availableQuantity > 0)
            {
                purchasedBooks.Add(new
                {
                    bookID = book.BookID,
                    title = book.Title,
                    author = book.Author,
                    isbn = book.ISBN,
                    edition = book.Edition,
                    condition = book.Condition,
                    quantity = availableQuantity,
                    purchaseDate = book.PurchaseDate,
                    orderID = book.OrderID
                });
            }
        }
        
        return Ok(purchasedBooks);
    }
    
    private class PurchasedBookInfo
    {
        public int BookID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string? Edition { get; set; }
        public string Condition { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int OrderID { get; set; }
    }
}

