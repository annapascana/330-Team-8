using CrimsonBookStore.Api.DTOs;
using CrimsonBookStore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrimsonBookStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBooks([FromQuery] BookSearchRequest? search)
    {
        if (search != null && (search.Title != null || search.Author != null || search.ISBN != null || search.MajorID != null || search.CourseID != null))
        {
            var books = await _bookService.SearchBooksAsync(search);
            return Ok(books);
        }
        
        var allBooks = await _bookService.GetAvailableBooksAsync();
        return Ok(allBooks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBook(int id)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        if (book == null)
        {
            return NotFound();
        }
        return Ok(book);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBook([FromBody] BookCreateRequest request)
    {
        // TODO: Add admin authorization check
        try
        {
            var book = await _bookService.CreateBookAsync(request);
            return CreatedAtAction(nameof(GetBook), new { id = book.BookID }, book);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(int id, [FromBody] BookUpdateRequest request)
    {
        // TODO: Add admin authorization check
        var success = await _bookService.UpdateBookAsync(id, request);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        // TODO: Add admin authorization check
        var success = await _bookService.DeleteBookAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}

