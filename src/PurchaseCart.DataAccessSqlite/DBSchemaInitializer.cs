using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace PurchaseCart.DataAccessSqlite;

public class DBSchemaInitializer
{
    private readonly string _connectionString;
    private readonly ILogger<DBSchemaInitializer> _logger;

    public DBSchemaInitializer(string connectionString, ILogger<DBSchemaInitializer> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public void Initialize()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Items (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Description TEXT NOT NULL
                );";

            command.ExecuteNonQuery();
            _logger.LogInformation("Items table initialized successfully");

            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Vat (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ItemId INTEGER NOT NULL,
                    Rate DECIMAL NOT NULL,
                    StartDate TEXT NOT NULL,
                    CONSTRAINT FK_Vat_Items FOREIGN KEY (ItemId) REFERENCES Items(Id)
                );";

            command.ExecuteNonQuery();
            _logger.LogInformation("Vat table initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Items table");
            throw;
        }
    }
} 