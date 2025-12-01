using CrimsonBookStore.Api.DTOs;

namespace CrimsonBookStore.Api.Services;

public interface ISellSubmissionService
{
    Task<SellSubmissionResponse> CreateSubmissionAsync(int userId, SellSubmissionRequest request);
    Task<List<SellSubmissionResponse>> GetSubmissionsByUserIdAsync(int userId);
    Task<List<SellSubmissionResponse>> GetAllSubmissionsAsync();
    Task<bool> ApproveSubmissionAsync(int submissionId);
    Task<bool> RejectSubmissionAsync(int submissionId);
}

