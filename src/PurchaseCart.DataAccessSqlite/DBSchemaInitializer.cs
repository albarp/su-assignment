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

            var tableDefinitions = new[]
            {
                (Name: "Items", Sql: @"
                    CREATE TABLE IF NOT EXISTS Items (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Description TEXT NOT NULL
                    )"),
                (Name: "Vat", Sql: @"
                    CREATE TABLE IF NOT EXISTS Vat (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ItemId INTEGER NOT NULL,
                        Rate DECIMAL NOT NULL,
                        StartDate TEXT NOT NULL,
                        CONSTRAINT FK_Vat_Items FOREIGN KEY (ItemId) REFERENCES Items(Id)
                    )"),
                (Name: "Pricing", Sql: @"
                    CREATE TABLE IF NOT EXISTS Pricing (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ItemId INTEGER NOT NULL,
                        Price DECIMAL NOT NULL,
                        StartDate TEXT NOT NULL,
                        CONSTRAINT FK_Pricing_Items FOREIGN KEY (ItemId) REFERENCES Items(Id)
                    )"),
                (Name: "Orders", Sql: @"
                    CREATE TABLE IF NOT EXISTS Orders (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Total DECIMAL NOT NULL,
                        TotalVat DECIMAL NOT NULL,
                        Date TEXT NOT NULL
                    )"),
                (Name: "OrderItems", Sql: @"
                    CREATE TABLE IF NOT EXISTS OrderItems (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        OrderId INTEGER NOT NULL,
                        ItemId INTEGER NOT NULL,
                        Quantity INTEGER NOT NULL,
                        Price DECIMAL NOT NULL,
                        VatValue DECIMAL NOT NULL,
                        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id),
                        CONSTRAINT FK_OrderItems_Items FOREIGN KEY (ItemId) REFERENCES Items(Id)
                    )")
            };

            foreach (var (tableName, sql) in tableDefinitions)
            {
                CreateTable(connection, tableName, sql);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing database schema");
            throw;
        }
    }

    private void CreateTable(SqliteConnection connection, string tableName, string sql)
    {
        try
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
            _logger.LogInformation("{TableName} table initialized successfully", tableName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing {TableName} table", tableName);
            throw;
        }
    }
} 