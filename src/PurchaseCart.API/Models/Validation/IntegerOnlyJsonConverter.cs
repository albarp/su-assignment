using System.Text.Json;
using System.Text.Json.Serialization;

namespace PurchaseCart.API.Models.Validation;

public class IntegerOnlyJsonConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                if (reader.TryGetDecimal(out decimal value))
                {
                    if (value != Math.Floor(value))
                    {
                        throw new JsonException("Value must be an integer");
                    }
                    return (int)value;
                }
                return reader.GetInt32();

            case JsonTokenType.String:
                throw new JsonException("Value must be a number, not a string");

            default:
                throw new JsonException($"Expected a number, but got {reader.TokenType}");
        }
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
} 