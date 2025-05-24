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

    public async Task<ItemOrderPrice[]> GetPricesAndVatsAsync(int[] ids)
    {
        if (ids == null || ids.Length == 0)
            return Array.Empty<ItemOrderPrice>();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // Create a parameterized query to get the latest price and VAT for each item
        var query = @"
            WITH LatestPrices AS (
                SELECT 
                    ItemId,
                    Price,
                    PriceDate
                FROM (
                    SELECT 
                        p.ItemId,
                        p.Price,
                        p.StartDate as PriceDate,
                        ROW_NUMBER() OVER (PARTITION BY p.ItemId ORDER BY p.StartDate DESC) as rn
                    FROM Pricing p
                    WHERE p.ItemId IN ({0})
                ) ranked
                WHERE rn = 1
            ),
            LatestVat AS (
                SELECT 
                    ItemId,
                    VatRate,
                    VatDate
                FROM (
                    SELECT 
                        v.ItemId,
                        v.Rate as VatRate,
                        v.StartDate as VatDate,
                        ROW_NUMBER() OVER (PARTITION BY v.ItemId ORDER BY v.StartDate DESC) as rn
                    FROM Vat v
                    WHERE v.ItemId IN ({0})
                ) ranked
                WHERE rn = 1
            )
            SELECT 
                lp.ItemId as Id,
                lp.Price,
                lv.VatRate
            FROM LatestPrices lp
            INNER JOIN LatestVat lv ON lp.ItemId = lv.ItemId";

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