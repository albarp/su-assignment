using Microsoft.Data.Sqlite;
using PurchaseCart.Domain.Entities;
using PurchaseCart.Domain.Interfaces;

namespace PurchaseCart.DataAccessSqlite;

public class ItemRepository : IItemRepository
{
    private readonly string _connectionString;

    public ItemRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<ItemOrderPrice[]> GetAllPricesAsync(int[] ids)
    {
        if (ids == null || ids.Length == 0)
            return Array.Empty<ItemOrderPrice>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // Create a parameterized query to get the latest price and VAT for each item
        var query = @"
            WITH LatestPrices AS (
                SELECT 
                    p.ItemId,
                    p.Price,
                    v.Rate as VatRate,
                    ROW_NUMBER() OVER (PARTITION BY p.ItemId ORDER BY p.StartDate DESC) as rn
                FROM Pricing p
                INNER JOIN Vat v ON p.ItemId = v.ItemId
                WHERE p.ItemId IN ({0})
                AND v.StartDate <= p.StartDate
            )
            SELECT 
                ItemId as Id,
                Price,
                VatRate
            FROM LatestPrices
            WHERE rn = 1";

        // Create the parameter placeholders for the IN clause
        var parameters = string.Join(",", ids.Select((_, i) => $"@id{i}"));
        query = string.Format(query, parameters);

        using var command = connection.CreateCommand();
        command.CommandText = query;

        // Add parameters for the IN clause
        for (int i = 0; i < ids.Length; i++)
        {
            command.Parameters.AddWithValue($"@id{i}", ids[i]);
        }

        var results = new List<ItemOrderPrice>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            results.Add(new ItemOrderPrice
            {
                Id = reader.GetInt32(0),
                Price = reader.GetDecimal(1),
                VatRate = reader.GetDecimal(2)
            });
        }

        return results.ToArray();
    }
} 