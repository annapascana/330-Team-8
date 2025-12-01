namespace CrimsonBookStore.Api.DTOs;

public class UserResponse
{
    public int UserID { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UserUpdateRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? UserType { get; set; }
}
