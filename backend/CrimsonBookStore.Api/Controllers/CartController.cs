using CrimsonBookStore.Api.DTOs;
using CrimsonBookStore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrimsonBookStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
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

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var sessionId = GetSessionId();
        var cart = await _cartService.GetCartAsync(sessionId);
        return Ok(cart);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var sessionId = GetSessionId();
        var success = await _cartService.AddToCartAsync(sessionId, request);
        if (!success)
        {
            return BadRequest(new { message = "Failed to add item to cart. Check stock availability." });
        }
        return Ok(new { message = "Item added to cart" });
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemRequest request)
    {
        var sessionId = GetSessionId();
        var success = await _cartService.UpdateCartItemAsync(sessionId, request);
        if (!success)
        {
            return BadRequest(new { message = "Failed to update cart item" });
        }
        return Ok(new { message = "Cart item updated" });
    }

    [HttpDelete("remove/{bookId}")]
    public async Task<IActionResult> RemoveFromCart(int bookId)
    {
        var sessionId = GetSessionId();
        var success = await _cartService.RemoveFromCartAsync(sessionId, bookId);
        if (!success)
        {
            return NotFound();
        }
        return Ok(new { message = "Item removed from cart" });
    }
}

