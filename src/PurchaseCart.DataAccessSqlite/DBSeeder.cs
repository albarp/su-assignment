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

            // Check if table is empty
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = "SELECT COUNT(*) FROM Items";
            var count = Convert.ToInt32(checkCommand.ExecuteScalar());

            if (count > 0)
            {
                _logger.LogInformation("Items table already contains data. Skipping seed.");
                return;
            }

            // Insert sample items
            var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @"
                INSERT INTO Items (Description) VALUES 
                ('Laptop'),
                ('Smartphone'),
                ('Headphones'),
                ('Mouse'),
                ('Keyboard');";

            insertCommand.ExecuteNonQuery();
            _logger.LogInformation("Items table seeded successfully with sample data");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding Items table");
            throw;
        }
    }
}
