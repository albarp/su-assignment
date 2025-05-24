using System.Text.Json.Serialization;

namespace PurchaseCart.API.Models;

public class OrderResponse
{
    [JsonPropertyName("order_id")]
    public int OrderId { get; set; }

    [JsonPropertyName("order_price")]
    public decimal OrderPrice { get; set; }

    [JsonPropertyName("order_vat")]
    public decimal OrderVat { get; set; }

    [JsonPropertyName("items")]
    public List<OrderItemResponse> Items { get; set; } = new();
}

public class OrderItemResponse
{
    [JsonPropertyName("product_id")]
    public int ProductId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("vat")]
    public decimal Vat { get; set; }
} 