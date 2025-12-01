using BCrypt.Net;
using CrimsonBookStore.Api.DTOs;
using CrimsonBookStore.Api.Repositories;

namespace CrimsonBookStore.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        // Check if user exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return null; // User already exists
        }

        existingUser = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUser != null)
        {
            return null; // Username taken
        }

        // Create new user - split username into FName and LName
        var nameParts = request.Username.Split(' ', 2);
        var user = new Api.Models.User
        {
            FName = nameParts.Length > 0 ? nameParts[0] : request.Username,
            LName = nameParts.Length > 1 ? nameParts[1] : "",
            Email = request.Email,
            PwdHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "Customer",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var userId = await _userRepository.CreateAsync(user);

        return new AuthResponse
        {
            UserID = userId,
            Username = user.Username,
            Email = user.Email,
            UserType = user.UserType,
            Token = $"user_{userId}" // Simple token for demo
        };
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PwdHash))
        {
            return null;
        }

        return new AuthResponse
        {
            UserID = user.UserID,
            Username = user.Username,
            Email = user.Email,
            UserType = user.UserType,
            Token = $"user_{user.UserID}"
        };
    }
}

