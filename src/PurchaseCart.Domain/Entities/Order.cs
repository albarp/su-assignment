namespace PurchaseCart.Domain.Entities;

public class Order
{
    public int? Id { get; set; }
    public decimal Total { get; set; }
    public decimal TotalVat { get; set; }
    public DateTime Date { get; set; }
    public required List<OrderItem> Items { get; set; }
}