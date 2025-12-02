namespace CrimsonBookStore.Api.Models;

public class SellSubmission
{
    public int SubID { get; set; }
    public int UserID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AuthTxt { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string? Edition { get; set; }
    public string Condition { get; set; } = "New";
    public decimal AskPrice { get; set; }
    public string Status { get; set; } = "Pending";
    public string? MajTxt { get; set; }
    public string? CrsTxt { get; set; }
    public decimal? AcqAgreed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    
    // Computed properties for backward compatibility
    public int SubmissionID => SubID;
    public string Author => AuthTxt;
    public decimal AskingPrice => AskPrice;
    public string SubmissionStatus => Status;
}

