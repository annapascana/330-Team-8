namespace CrimsonBookStore.Api.Models;

public class Book
{
    public int BookID { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Edition { get; set; }
    public string Condition { get; set; } = "New";
    public decimal AcqCost { get; set; }
    public decimal SellPrice { get; set; }
    public int StockQty { get; set; }
    public string Status { get; set; } = "Available";
    public int? SubID { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Computed properties for backward compatibility
    public decimal AcquisitionCost => AcqCost;
    public decimal SellingPrice => SellPrice;
    public int StockQuantity => StockQty;
    
    // Author will come from AuthoredBy junction table
    public string Author { get; set; } = string.Empty; // For display, populated via JOIN
}

