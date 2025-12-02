namespace CrimsonBookStore.Api.DTOs;

public class UserResponse
{
    public int UserID { get; set; }
    public string FName { get; set; } = string.Empty;
    public string LName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Computed property for backward compatibility
    public string Username => $"{FName} {LName}";
}

public class UserUpdateRequest
{
    public string? FName { get; set; }
    public string? LName { get; set; }
    public string? Email { get; set; }
    public string? UserType { get; set; }
    
    // Backward compatibility - if Username is provided, split it
    public string? Username 
    { 
        get => null; 
        set 
        { 
            if (!string.IsNullOrEmpty(value))
            {
                var nameParts = value.Split(' ', 2);
                FName = nameParts.Length > 0 ? nameParts[0] : value;
                LName = nameParts.Length > 1 ? nameParts[1] : "";
            }
        } 
    }
}
