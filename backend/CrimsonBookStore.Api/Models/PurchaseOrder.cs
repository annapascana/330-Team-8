namespace CrimsonBookStore.Api.Models;

public class PurchaseOrder
{
    public int POID { get; set; }
    public int UserID { get; set; }
    public int BookID { get; set; } // From ERD - though POItem handles multiple books
    public string Status { get; set; } = "New";
    public decimal SubTot { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public DateTime UpdAt { get; set; }
    public List<OrderLineItem> LineItems { get; set; } = new();
    
    // Computed properties for backward compatibility
    public DateTime OrderDate => UpdAt;
    public decimal SubTotal => SubTot;
    public DateTime UpdatedAt => UpdAt;
    public DateTime? CancelledAt => Status == "Cancelled" ? UpdAt : null;
}

public class OrderLineItem
{
    public int POID { get; set; }
    public int LineNo { get; set; }
    public int BookID { get; set; }
    public int Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTot { get; set; }
    public Book? Book { get; set; }
    
    // Computed properties for backward compatibility
    public int LineItemID => LineNo;
    public int Quantity => Qty;
    public decimal LineTotal => LineTot;
}

