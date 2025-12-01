namespace CrimsonBookStore.Api.DTOs;

public class SellSubmissionRequest
{
    public int UserID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string? Edition { get; set; }
    public string Condition { get; set; } = "New";
    public decimal AskingPrice { get; set; }
}

public class SellSubmissionResponse
{
    public int SubmissionID { get; set; }
    public int UserID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string? Edition { get; set; }
    public string Condition { get; set; } = string.Empty;
    public decimal AskingPrice { get; set; }
    public string SubmissionStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

