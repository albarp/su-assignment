using PurchaseCart.Domain;
using PurchaseCart.Domain.Interfaces;
using PurchaseCart.Domain.Logic;
using PurchaseCart.Domain.Requests;
using PurchaseCart.Domain.Entities;
using Moq;
using Xunit;

namespace PurchaseCart.Tests.Unit;

[Trait("Category", "Unit")]
public class OrderLogicTests
{
    private readonly Mock<IItemRepository> _mockItemRepository;
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly OrderLogic _orderLogic;

    public OrderLogicTests()
    {
        _mockItemRepository = new Mock<IItemRepository>();
        _mockOrderRepository = new Mock<IOrderRepository>();
        _orderLogic = new OrderLogic(_mockItemRepository.Object, _mockOrderRepository.Object);
    }

    [Fact]
    public async Task CalcAndSave_MissingPrices_ThrowsArgumentException()
    {
        // Arrange
        var orderRequest = new PurchaseCart.Domain.Requests.OrderRequest
        {
            Order = new PurchaseCart.Domain.Requests.Order
            {
                Items = new List<PurchaseCart.Domain.Requests.OrderItem>
                {
                    new PurchaseCart.Domain.Requests.OrderItem { ProductId = 1, Quantity = 1 }
                }
            }
        };

        _mockItemRepository
            .Setup(repo => repo.GetPricesAndVatsAsync(It.IsAny<int[]>()))
            .ReturnsAsync(Array.Empty<PurchaseCart.Domain.Entities.ItemOrderPrice>());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _orderLogic.CalcAndSave(orderRequest));
        _mockItemRepository.Verify(repo => repo.GetPricesAndVatsAsync(It.IsAny<int[]>()), Times.Once);
        _mockOrderRepository.Verify(repo => repo.Save(It.IsAny<PurchaseCart.Domain.Entities.Order>()), Times.Never);
    }

    [Fact]
    public async Task CalcAndSave_ValidItems_CalculatesAndSavesOrder()
    {
        // Arrange
        var orderRequest = new Domain.Requests.OrderRequest
        {
            Order = new Domain.Requests.Order
            {
                Items = new List<Domain.Requests.OrderItem>
                {
                    new Domain.Requests.OrderItem { ProductId = 1, Quantity = 2 },
                    new Domain.Requests.OrderItem { ProductId = 2, Quantity = 1 }
                }
            }
        };

        var itemPrices = new[]
        {
            new ItemOrderPrice { Id = 1, Price = 10.0m, VatRate = 0.2m },
            new ItemOrderPrice { Id = 2, Price = 15.0m, VatRate = 0.2m }
        };

        _mockItemRepository
            .Setup(repo => repo.GetPricesAndVatsAsync(It.IsAny<int[]>()))
            .ReturnsAsync(itemPrices);

        var savedOrder = new Domain.Entities.Order
        {
            Items = new List<Domain.Entities.OrderItem>()
        };

        _mockOrderRepository
            .Setup(repo => repo.Save(It.IsAny<Domain.Entities.Order>()))
            .Callback<Domain.Entities.Order>(order => 
            {
                savedOrder.Total = order.Total;
                savedOrder.TotalVat = order.TotalVat;
                savedOrder.Items = order.Items;
                savedOrder.Date = order.Date;
            })
            .ReturnsAsync(savedOrder);

        // Act
        var result = await _orderLogic.CalcAndSave(orderRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(35.0m, result.Total); // (2 * 10) + (1 * 15)
        Assert.Equal(7.0m, result.TotalVat); // (20 * 0.2) + (15 * 0.2)
        Assert.Equal(2, result.Items.Count);
        
        var firstItem = result.Items.First(i => i.ItemId == 1);
        Assert.Equal(2, firstItem.Quantity);
        Assert.Equal(20.0m, firstItem.Price);
        Assert.Equal(4.0m, firstItem.VatValue);

        var secondItem = result.Items.First(i => i.ItemId == 2);
        Assert.Equal(1, secondItem.Quantity);
        Assert.Equal(15.0m, secondItem.Price);
        Assert.Equal(3.0m, secondItem.VatValue);

        _mockItemRepository.Verify(repo => repo.GetPricesAndVatsAsync(It.Is<int[]>(ids => ids.Length == 2)), Times.Once);
        _mockOrderRepository.Verify(repo => repo.Save(It.IsAny<Domain.Entities.Order>()), Times.Once);
    }
} 