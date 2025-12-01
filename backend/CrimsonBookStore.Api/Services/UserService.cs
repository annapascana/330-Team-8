using CrimsonBookStore.Api.DTOs;
using CrimsonBookStore.Api.Repositories;

namespace CrimsonBookStore.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserResponse>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => new UserResponse
        {
            UserID = u.UserID,
            Username = $"{u.FName} {u.LName}",
            Email = u.Email,
            UserType = u.Role,
            CreatedAt = u.CreatedAt
        }).ToList();
    }

    public async Task<UserResponse?> GetUserByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        return new UserResponse
        {
            UserID = user.UserID,
            Username = $"{user.FName} {user.LName}",
            Email = user.Email,
            UserType = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> UpdateUserAsync(int userId, UserUpdateRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        if (request.Username != null)
        {
            var nameParts = request.Username.Split(' ', 2);
            user.FName = nameParts.Length > 0 ? nameParts[0] : request.Username;
            user.LName = nameParts.Length > 1 ? nameParts[1] : "";
        }
        if (request.Email != null) user.Email = request.Email;
        if (request.UserType != null) user.Role = request.UserType;

        return await _userRepository.UpdateAsync(user);
    }
}

