using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace PurchaseCart.DataAccessSqlite;

public class DBSeeder
{
    private readonly string _connectionString;
    private readonly ILogger<DBSeeder> _logger;

    public DBSeeder(string connectionString, ILogger<DBSeeder> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public void Seed()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            SeedTable(connection, "Items", @"
                INSERT INTO Items (Description) VALUES 
                ('Laptop'),
                ('Smartphone'),
                ('Headphones'),
                ('Mouse'),
                ('Keyboard');");

            SeedTable(connection, "Pricing", @"
                INSERT INTO Pricing (ItemId, Price, StartDate) VALUES 
                (1, 999.99, '2024-01-01'),
                (2, 699.99, '2024-01-01'),
                (3, 149.99, '2024-01-01'),
                (4, 29.99, '2024-01-01'),
                (5, 79.99, '2024-01-01');");

            SeedTable(connection, "Vat", @"
                INSERT INTO Vat (ItemId, Rate, StartDate) VALUES 
                (1, 0.1, '2024-01-01'),
                (2, 0.1, '2024-01-01'),
                (3, 0.1, '2024-01-01'),
                (4, 0.1, '2024-01-01'),
                (5, 0.1, '2024-01-01');");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding tables");
            throw;
        }
    }

    private void SeedTable(SqliteConnection connection, string tableName, string insertSql)
    {
        var checkCommand = connection.CreateCommand();
        checkCommand.CommandText = $"SELECT COUNT(*) FROM {tableName}";
        var count = Convert.ToInt32(checkCommand.ExecuteScalar());

        if (count > 0)
        {
            _logger.LogInformation($"{tableName} table already contains data. Skipping seed.");
            return;
        }

        var insertCommand = connection.CreateCommand();
        insertCommand.CommandText = insertSql;
        insertCommand.ExecuteNonQuery();
        _logger.LogInformation($"{tableName} table seeded successfully with sample data");
    }
}
