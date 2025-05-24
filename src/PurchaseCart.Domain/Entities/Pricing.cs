namespace PurchaseCart.Domain.Entities;

public class Pricing
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
}
