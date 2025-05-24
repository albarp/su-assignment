namespace PurchaseCart.Domain.Entities;

public class Vat
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public decimal Rate { get; set; }
    public DateTime StartDate { get; set; }
}
