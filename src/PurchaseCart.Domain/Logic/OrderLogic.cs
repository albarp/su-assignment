namespace PurchaseCart.Domain.Logic;

using PurchaseCart.Domain.Requests;
using PurchaseCart.Domain.Interfaces;

public class OrderLogic {
    private IItemRepository _itemRepository;
    private IOrderRepository _orderRepository;

    public OrderLogic(IItemRepository itemRepository, IOrderRepository orderRepository){
        _itemRepository = itemRepository;
        _orderRepository = orderRepository;
    }

    public async Task<Domain.Entities.Order?> CalcAndSave(OrderRequest orderRequest) {
        if (orderRequest?.Order?.Items == null || !orderRequest.Order.Items.Any())
        {
            return null;
        }

        // Get all product IDs from the order request
        var itemIds = orderRequest.Order.Items.Select(item => item.ProductId).ToArray();

        // Get prices and VAT rates for all items
        var itemPrices = await _itemRepository.GetPricesAndVatsAsync(itemIds);

        // Validate that we have prices for all requested items
        var missingPrices = itemIds.Except(itemPrices.Select(p => p.Id)).ToArray();
        if (missingPrices.Any())
        {
            throw new ArgumentException($"No prices found for product IDs: {string.Join(", ", missingPrices)}");
        }

        // Create order items with calculated prices and VAT
        var orderItems = new List<Domain.Entities.OrderItem>();
        decimal totalOrderPrice = 0;
        decimal totalOrderVat = 0;

        foreach (var requestItem in orderRequest.Order.Items)
        {
            // Since we validated above, we know there's exactly one price per item
            var itemPrice = itemPrices.Single(p => p.Id == requestItem.ProductId);

            var itemTotalPrice = itemPrice.Price * requestItem.Quantity;
            var itemVatValue = itemTotalPrice * itemPrice.VatRate;

            orderItems.Add(new Domain.Entities.OrderItem
            {
                ItemId = requestItem.ProductId,
                Quantity = requestItem.Quantity,
                Price = itemTotalPrice,
                VatValue = itemVatValue
            });

            totalOrderPrice += itemTotalPrice;
            totalOrderVat += itemVatValue;
        }

        // Create and save the order
        var order = new Domain.Entities.Order
        {
            Total = totalOrderPrice,
            TotalVat = totalOrderVat,
            Date = DateTime.UtcNow,
            Items = orderItems
        };

        return await _orderRepository.Save(order);
    }
}