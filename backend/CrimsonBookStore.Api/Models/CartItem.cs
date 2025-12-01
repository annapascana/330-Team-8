namespace CrimsonBookStore.Api.Models;

public class CartItem
{
    public int BookID { get; set; }
    public int Quantity { get; set; }
    public Book? Book { get; set; }
}

