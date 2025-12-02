using CrimsonBookStore.Api.DTOs;
using CrimsonBookStore.Api.Repositories;

namespace CrimsonBookStore.Api.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<List<BookResponse>> GetAvailableBooksAsync()
    {
        var books = await _bookRepository.GetAllAvailableAsync();
        return books.Select(b => MapToResponse(b)).ToList();
    }

    public async Task<BookResponse?> GetBookByIdAsync(int bookId)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        return book != null ? MapToResponse(book) : null;
    }

    public async Task<List<BookResponse>> SearchBooksAsync(BookSearchRequest request)
    {
        var books = await _bookRepository.SearchAsync(
            request.Title, request.Author, request.ISBN, request.MajorID, request.CourseID);
        return books.Select(b => MapToResponse(b)).ToList();
    }

    public async Task<BookResponse> CreateBookAsync(BookCreateRequest request)
    {
        // Business rule: SellingPrice must be greater than AcquisitionCost
        if (request.SellingPrice <= request.AcquisitionCost)
        {
            throw new Exception("Selling price must be greater than acquisition cost");
        }

        var book = new Api.Models.Book
        {
            ISBN = request.ISBN,
            Title = request.Title,
            Edition = request.Edition,
            Condition = request.Condition,
            AcqCost = request.AcquisitionCost,
            SellPrice = request.SellingPrice,
            StockQty = request.StockQuantity,
            Status = request.StockQuantity > 0 ? "Available" : "Unavailable",
            CreatedAt = DateTime.UtcNow
        };
        
        // Note: Author and Course/Major relationships handled via junction tables
        // For now, we'll store author name in the Author property for display
        book.Author = request.Author ?? "";

        var bookId = await _bookRepository.CreateAsync(book);
        book.BookID = bookId;
        return MapToResponse(book);
    }

    public async Task<bool> UpdateBookAsync(int bookId, BookUpdateRequest request)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);
        if (book == null) return false;

        if (request.Title != null) book.Title = request.Title;
        if (request.Edition != null) book.Edition = request.Edition;
        if (request.Condition != null) book.Condition = request.Condition;
        if (request.AcquisitionCost.HasValue) book.AcqCost = request.AcquisitionCost.Value;
        if (request.SellingPrice.HasValue) book.SellPrice = request.SellingPrice.Value;
        if (request.StockQuantity.HasValue)
        {
            book.StockQty = request.StockQuantity.Value;
            book.Status = request.StockQuantity.Value > 0 ? "Available" : "Unavailable";
        }
        if (request.Status != null) book.Status = request.Status;

        // Business rule: SellingPrice must be greater than AcquisitionCost
        var finalAcquisitionCost = request.AcquisitionCost ?? book.AcqCost;
        var finalSellingPrice = request.SellingPrice ?? book.SellPrice;
        if (finalSellingPrice <= finalAcquisitionCost)
        {
            throw new Exception("Selling price must be greater than acquisition cost");
        }

        return await _bookRepository.UpdateAsync(book);
    }

    public async Task<bool> DeleteBookAsync(int bookId)
    {
        return await _bookRepository.DeleteAsync(bookId);
    }

    private static BookResponse MapToResponse(Api.Models.Book book)
    {
        return new BookResponse
        {
            BookID = book.BookID,
            ISBN = book.ISBN,
            Title = book.Title,
            Author = book.Author, // Populated from AuthoredBy join
            Edition = book.Edition,
            Condition = book.Condition,
            SellingPrice = book.SellPrice,
            StockQuantity = book.StockQty,
            Status = book.Status,
            MajorID = null, // Will need to query UsedIn junction table
            CourseID = null // Will need to query UsedIn junction table
        };
    }
}

