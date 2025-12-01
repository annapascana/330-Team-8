using CrimsonBookStore.Api.DTOs;

namespace CrimsonBookStore.Api.Services;

public interface IBookService
{
    Task<List<BookResponse>> GetAvailableBooksAsync();
    Task<BookResponse?> GetBookByIdAsync(int bookId);
    Task<List<BookResponse>> SearchBooksAsync(BookSearchRequest request);
    Task<BookResponse> CreateBookAsync(BookCreateRequest request);
    Task<bool> UpdateBookAsync(int bookId, BookUpdateRequest request);
    Task<bool> DeleteBookAsync(int bookId);
}

