namespace PurchaseCart.Domain.Interfaces;

using PurchaseCart.Domain.Entities;
public interface IOrderRepository
{
    Task<Order> Save(Order order);
}
