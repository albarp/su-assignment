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
} 