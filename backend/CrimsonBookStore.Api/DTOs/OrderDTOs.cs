namespace CrimsonBookStore.Api.DTOs;

public class CheckoutRequest
{
    public int UserID { get; set; }
    // Cart items are in session, no need to pass them
}

public class OrderResponse
{
    public int POID { get; set; }
    public int UserID { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public DateTime? CancelledAt { get; set; }
    public List<OrderLineItemResponse> LineItems { get; set; } = new();
}

public class OrderLineItemResponse
{
    public int LineItemID { get; set; }
    public int BookID { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? ISBN { get; set; }
    public string? Edition { get; set; }
    public string? Condition { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

