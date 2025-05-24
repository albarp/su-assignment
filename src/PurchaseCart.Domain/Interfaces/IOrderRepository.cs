namespace PurchaseCart.Domain.Interfaces;

using PurchaseCart.Domain;
public interface IOrderRepository
{
    Task<Order> Save(Order order);
}
