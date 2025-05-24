namespace PurchaseCart.Domain.Interfaces;

using PurchaseCart.Domain.Entities;
public interface IItemRepository
{
    Task<ItemOrderPrice[]> GetPricesAndVatsAsync(int[] ids);
}
