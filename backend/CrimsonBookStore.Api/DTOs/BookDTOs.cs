namespace CrimsonBookStore.Api.DTOs;

public class BookResponse
{
    public int BookID { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Edition { get; set; }
    public string Condition { get; set; } = string.Empty;
    public decimal SellingPrice { get; set; }
    public int StockQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? MajorID { get; set; }
    public int? CourseID { get; set; }
}

public class BookCreateRequest
{
    public string ISBN { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Edition { get; set; }
    public string Condition { get; set; } = "New";
    public decimal AcquisitionCost { get; set; }
    public decimal SellingPrice { get; set; }
    public int StockQuantity { get; set; }
    public int? MajorID { get; set; }
    public int? CourseID { get; set; }
}

public class BookUpdateRequest
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Edition { get; set; }
    public string? Condition { get; set; }
    public decimal? AcquisitionCost { get; set; }
    public decimal? SellingPrice { get; set; }
    public int? StockQuantity { get; set; }
    public string? Status { get; set; }
    public int? MajorID { get; set; }
    public int? CourseID { get; set; }
}

public class BookSearchRequest
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? ISBN { get; set; }
    public int? MajorID { get; set; }
    public int? CourseID { get; set; }
}

