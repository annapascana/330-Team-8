using CrimsonBookStore.Api.DTOs;

namespace CrimsonBookStore.Api.Services;

public interface IUserService
{
    Task<List<UserResponse>> GetAllUsersAsync();
    Task<UserResponse?> GetUserByIdAsync(int userId);
    Task<bool> UpdateUserAsync(int userId, UserUpdateRequest request);
}

