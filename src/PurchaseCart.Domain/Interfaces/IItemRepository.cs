namespace PurchaseCart.Domain.Interfaces;

using PurchaseCart.Domain.Entities;
public interface IItemRepository
{
    Task<ItemOrderPrice[]> GetAllPricesAsync(int[] ids);
}
