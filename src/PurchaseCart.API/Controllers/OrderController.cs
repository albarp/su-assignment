using Microsoft.AspNetCore.Mvc;
using PurchaseCart.Domain.Logic;
using PurchaseCart.Domain.Requests;
using PurchaseCart.API.Models;

namespace PurchaseCart.API.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;
    private readonly OrderLogic _orderLogic;

    public OrderController(ILogger<OrderController> logger, OrderLogic orderLogic)
    {
        _logger = logger;
        _orderLogic = orderLogic;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] Models.OrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("Received order with {Count} items", request.Order.Items.Count);
            
            // Map API model to Domain model
            var domainRequest = new Domain.Requests.OrderRequest
            {
                Order = new Domain.Requests.Order
                {
                    Items = request.Order.Items.Select(item => new Domain.Requests.OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    }).ToList()
                }
            };
            
            var order = await _orderLogic.CalcAndSave(domainRequest);
            
            if (order == null)
            {
                return BadRequest("Invalid order request - no items provided");
            }

            // Map domain model to response model
            var response = new OrderResponse
            {
                OrderId = order.Id ?? 0,
                OrderPrice = order.Total,
                OrderVat = order.TotalVat,
                Items = order.Items.Select(item => new OrderItemResponse
                {
                    ProductId = item.ItemId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Vat = item.VatValue
                }).ToList()
            };

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid order request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order");
            return StatusCode(500, "An error occurred while processing your order");
        }
    }
}
