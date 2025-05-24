using PurchaseCart.Domain;
using Xunit;

namespace PurchaseCart.Tests.Unit;

[Trait("Category", "Unit")]
public class OrderTests
{
    [Fact]
    public void CreateOrder_WithValidData_Succeeds()
    {
        // Arrange
        var order = new Order
        {
            Total = 100.00m,
            TotalVat = 20.00m,
            Date = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new()
                {
                    ItemId = 1,
                    Quantity = 1,
                    Price = 80.00m,
                    VatValue = 20.00m
                }
            }
        };

        // Act & Assert
        Assert.NotNull(order);
        Assert.Single(order.Items);
        Assert.Equal(100.00m, order.Total);
        Assert.Equal(20.00m, order.TotalVat);
    }
} 