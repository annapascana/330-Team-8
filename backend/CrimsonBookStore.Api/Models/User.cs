namespace CrimsonBookStore.Api.Models;

public class User
{
    public int UserID { get; set; }
    public string FName { get; set; } = string.Empty;
    public string LName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PwdHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    
    // Computed properties for backward compatibility
    public string Username => $"{FName} {LName}";
    public string UserType => Role;
    public string PasswordHash => PwdHash;
}

