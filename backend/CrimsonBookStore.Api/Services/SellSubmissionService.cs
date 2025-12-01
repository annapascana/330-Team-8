using CrimsonBookStore.Api.DTOs;
using CrimsonBookStore.Api.Repositories;

namespace CrimsonBookStore.Api.Services;

public class SellSubmissionService : ISellSubmissionService
{
    private readonly ISellSubmissionRepository _submissionRepository;
    private readonly IBookRepository _bookRepository;

    public SellSubmissionService(ISellSubmissionRepository submissionRepository, IBookRepository bookRepository)
    {
        _submissionRepository = submissionRepository;
        _bookRepository = bookRepository;
    }

    public async Task<SellSubmissionResponse> CreateSubmissionAsync(int userId, SellSubmissionRequest request)
    {
        var submission = new Api.Models.SellSubmission
        {
            UserID = userId,
            Title = request.Title,
            AuthTxt = request.Author,
            ISBN = request.ISBN,
            Edition = request.Edition,
            Condition = request.Condition,
            AskPrice = request.AskingPrice,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        var submissionId = await _submissionRepository.CreateAsync(submission);
        submission.SubID = submissionId;
        return MapToResponse(submission);
    }

    public async Task<List<SellSubmissionResponse>> GetSubmissionsByUserIdAsync(int userId)
    {
        var submissions = await _submissionRepository.GetByUserIdAsync(userId);
        return submissions.Select(MapToResponse).ToList();
    }

    public async Task<List<SellSubmissionResponse>> GetAllSubmissionsAsync()
    {
        var submissions = await _submissionRepository.GetAllAsync();
        return submissions.Select(MapToResponse).ToList();
    }

    public async Task<bool> ApproveSubmissionAsync(int submissionId)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId);
        if (submission == null || submission.Status != "Pending")
        {
            return false;
        }

        // Check if book with this ISBN already exists
        var existingBook = await _bookRepository.GetByISBNAsync(submission.ISBN);
        
        if (existingBook != null)
        {
            // Update existing book stock
            await _bookRepository.UpdateStockAsync(existingBook.BookID, 1);
        }
        else
        {
            // Create new book
            var book = new Api.Models.Book
            {
                ISBN = submission.ISBN,
                Title = submission.Title,
                Edition = submission.Edition,
                Condition = submission.Condition,
                AcqCost = submission.AskPrice * 0.7m, // 70% of asking price
                SellPrice = submission.AskPrice,
                StockQty = 1,
                Status = "Available",
                SubID = submission.SubID,
                CreatedAt = DateTime.UtcNow
            };
            var bookId = await _bookRepository.CreateAsync(book);
            
            // TODO: Create Author record and AuthoredBy relationship if needed
        }

        // Update submission status
        submission.Status = "Approved";
        submission.ReviewedAt = DateTime.UtcNow;
        return await _submissionRepository.UpdateAsync(submission);
    }

    public async Task<bool> RejectSubmissionAsync(int submissionId)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId);
        if (submission == null || submission.Status != "Pending")
        {
            return false;
        }

        submission.Status = "Rejected";
        submission.ReviewedAt = DateTime.UtcNow;
        return await _submissionRepository.UpdateAsync(submission);
    }

    private static SellSubmissionResponse MapToResponse(Api.Models.SellSubmission submission)
    {
        return new SellSubmissionResponse
        {
            SubmissionID = submission.SubID,
            UserID = submission.UserID,
            Title = submission.Title,
            Author = submission.AuthTxt,
            ISBN = submission.ISBN,
            Edition = submission.Edition,
            Condition = submission.Condition,
            AskingPrice = submission.AskPrice,
            SubmissionStatus = submission.Status,
            CreatedAt = submission.CreatedAt,
            ReviewedAt = submission.ReviewedAt
        };
    }
}

