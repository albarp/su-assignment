using Microsoft.AspNetCore.Mvc;

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

    [HttpGet(Name = "GetOrders")]
    public IEnumerable<string> Get()
    {
        return Orders;
    }
}
