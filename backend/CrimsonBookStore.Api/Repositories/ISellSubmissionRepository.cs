using CrimsonBookStore.Api.Models;

namespace CrimsonBookStore.Api.Repositories;

public interface ISellSubmissionRepository
{
    Task<List<SellSubmission>> GetByUserIdAsync(int userId);
    Task<List<SellSubmission>> GetAllAsync();
    Task<SellSubmission?> GetByIdAsync(int submissionId);
    Task<int> CreateAsync(SellSubmission submission);
    Task<bool> UpdateAsync(SellSubmission submission);
    Task<int> CountApprovedByUserAndISBNAsync(int userId, string isbn);
}

