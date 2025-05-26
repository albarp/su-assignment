using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PurchaseCart.API.Models.Validation;

namespace PurchaseCart.API.Models;

public class OrderRequest
{
    [Required]
    public Order Order { get; set; } = null!;
}

public class Order
{
    [Required]
    [MinLength(1, ErrorMessage = "Order must contain at least one item")]
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    [Required(ErrorMessage = "ProductId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0")]
    [JsonConverter(typeof(IntegerOnlyJsonConverter))]
    [JsonPropertyName("product_id")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    [JsonConverter(typeof(IntegerOnlyJsonConverter))]
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
} 