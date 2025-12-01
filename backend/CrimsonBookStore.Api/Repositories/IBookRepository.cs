using CrimsonBookStore.Api.Models;

namespace CrimsonBookStore.Api.Repositories;

public interface IBookRepository
{
    Task<List<Book>> GetAllAvailableAsync();
    Task<Book?> GetByIdAsync(int bookId);
    Task<List<Book>> SearchAsync(string? title, string? author, string? isbn, int? majorId, int? courseId);
    Task<int> CreateAsync(Book book);
    Task<bool> UpdateAsync(Book book);
    Task<bool> DeleteAsync(int bookId);
    Task<bool> UpdateStockAsync(int bookId, int quantityChange);
    Task<Book?> GetByISBNAsync(string isbn);
}

