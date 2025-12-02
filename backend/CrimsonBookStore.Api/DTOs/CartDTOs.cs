namespace CrimsonBookStore.Api.DTOs;

public class AddToCartRequest
{
    public int BookID { get; set; }
    public int Quantity { get; set; }
}

public class UpdateCartItemRequest
{
    public int BookID { get; set; }
    public int Quantity { get; set; }
}

public class CartItemResponse
{
    public int BookID { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public int AvailableStock { get; set; }
}

public class CartResponse
{
    public List<CartItemResponse> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
}

