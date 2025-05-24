namespace PurchaseCart.Domain.Entities;

public class OrderRequest
{
    public Order Order { get; set; } = null!;
}

public class Order
{
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
} 