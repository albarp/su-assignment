namespace PurchaseCart.Domain.Entities;

// TODO: Since it is not persisted, move away from the Entities namepace
public class ItemOrderPrice
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public decimal VatRate { get; set; }
}
