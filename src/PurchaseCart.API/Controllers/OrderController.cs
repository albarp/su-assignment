using Microsoft.AspNetCore.Mvc;
using PurchaseCart.API.Models;

namespace PurchaseCart.API.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private static readonly string[] Orders = new[]
    {
        "Order1", "Order2", "Order3"
    };

    private readonly ILogger<OrderController> _logger;

    public OrderController(ILogger<OrderController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public IActionResult CreateOrder([FromBody] OrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Received order with {Count} items", request.Order.Items.Count);

        return Ok(new { message = "Order received successfully" });
    }
}
