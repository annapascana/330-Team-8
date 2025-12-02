using CrimsonBookStore.Api.DTOs;
using CrimsonBookStore.Api.Repositories;

namespace CrimsonBookStore.Api.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IBookRepository _bookRepository;

    public CartService(ICartRepository cartRepository, IBookRepository bookRepository)
    {
        _cartRepository = cartRepository;
        _bookRepository = bookRepository;
    }

    public async Task<CartResponse> GetCartAsync(string sessionId)
    {
        var cartItems = await _cartRepository.GetCartItemsAsync(sessionId);
        var response = new CartResponse();

        foreach (var item in cartItems)
        {
            if (item.Book == null)
            {
                item.Book = await _bookRepository.GetByIdAsync(item.BookID);
            }

            if (item.Book != null && item.Book.StockQty >= item.Quantity)
            {
                response.Items.Add(new CartItemResponse
                {
                    BookID = item.BookID,
                    Title = item.Book.Title,
                    Author = item.Book.Author,
                    UnitPrice = item.Book.SellPrice,
                    Quantity = item.Quantity,
                    LineTotal = item.Book.SellPrice * item.Quantity,
                    AvailableStock = item.Book.StockQty
                });
            }
        }

        response.SubTotal = response.Items.Sum(i => i.LineTotal);
        response.Tax = response.SubTotal * 0.08m; // 8% tax
        response.Total = response.SubTotal + response.Tax;

        return response;
    }

    public async Task<bool> AddToCartAsync(string sessionId, AddToCartRequest request)
    {
        var book = await _bookRepository.GetByIdAsync(request.BookID);
        if (book == null || book.Status != "Available" || book.StockQty < request.Quantity)
        {
            return false;
        }

        var cartItems = await _cartRepository.GetCartItemsAsync(sessionId);
        var existingItem = cartItems.FirstOrDefault(x => x.BookID == request.BookID);
        var totalQuantity = (existingItem?.Quantity ?? 0) + request.Quantity;

        if (totalQuantity > book.StockQty)
        {
            return false;
        }

        var cartItem = new Api.Models.CartItem
        {
            BookID = request.BookID,
            Quantity = request.Quantity,
            Book = book
        };

        return await _cartRepository.AddItemAsync(sessionId, cartItem);
    }

    public async Task<bool> UpdateCartItemAsync(string sessionId, UpdateCartItemRequest request)
    {
        var book = await _bookRepository.GetByIdAsync(request.BookID);
        if (book == null || book.StockQty < request.Quantity)
        {
            return false;
        }

        return await _cartRepository.UpdateItemAsync(sessionId, request.BookID, request.Quantity);
    }

    public async Task<bool> RemoveFromCartAsync(string sessionId, int bookId)
    {
        return await _cartRepository.RemoveItemAsync(sessionId, bookId);
    }

    public async Task<bool> ClearCartAsync(string sessionId)
    {
        return await _cartRepository.ClearCartAsync(sessionId);
    }
}

