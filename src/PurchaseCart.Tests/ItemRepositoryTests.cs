using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using PurchaseCart.DataAccessSqlite;
using PurchaseCart.Domain.Entities;
using Xunit;

namespace PurchaseCart.IntegrationTests;

public class ItemRepositoryTests : IDisposable
{
    private readonly string _connectionString;
    private readonly SqliteConnection _connection;
    private readonly ItemRepository _repository;
    private readonly ILogger<DBSchemaInitializer> _logger;

    public ItemRepositoryTests()
    {
        // Create a unique database file for each test run
        var dbPath = Path.Combine(Path.GetTempPath(), $"integration_test_{Guid.NewGuid()}.db");
        _connectionString = $"Data Source={dbPath}";
        _connection = new SqliteConnection(_connectionString);
        _connection.Open();
        
        // Create a test logger
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<DBSchemaInitializer>();
        
        // Initialize schema
        var initializer = new DBSchemaInitializer(_connectionString, _logger);
        initializer.Initialize();
        
        _repository = new ItemRepository(_connectionString);
    }

    [Fact]
    public async Task GetAllPricesAsync_ReturnsCorrectPrices()
    {
        // Arrange
        var insertItems = @"
            INSERT INTO Items (Id, Description) VALUES 
            (1, 'Test Item 1'),
            (2, 'Test Item 2')";
        
        var insertVat = @"
            INSERT INTO Vat (ItemId, Rate, StartDate) VALUES 
            (1, 0.20, '2024-01-01'),
            (2, 0.10, '2024-01-01')";
        
        var insertPricing = @"
            INSERT INTO Pricing (ItemId, Price, StartDate) VALUES 
            (1, 100.00, '2024-01-01'),
            (2, 200.00, '2024-01-01')";
        
        using var command = _connection.CreateCommand();
        command.CommandText = insertItems;
        command.ExecuteNonQuery();
        
        command.CommandText = insertVat;
        command.ExecuteNonQuery();
        
        command.CommandText = insertPricing;
        command.ExecuteNonQuery();

        try
        {
            var itemIds = new[] { 1, 2 };

            // Act
            var results = await _repository.GetPricesAndVatsAsync(itemIds);

            // Assert
            Assert.Equal(2, results.Length);
            
            var item1 = results.First(r => r.Id == 1);
            Assert.Equal(100.00m, item1.Price);
            Assert.Equal(0.20m, item1.VatRate);

            var item2 = results.First(r => r.Id == 2);
            Assert.Equal(200.00m, item2.Price);
            Assert.Equal(0.10m, item2.VatRate);
        }
        finally
        {
            // Clean up test data
            var cleanup = @"
                DELETE FROM Pricing;
                DELETE FROM Vat;
                DELETE FROM Items;";
            
            command.CommandText = cleanup;
            command.ExecuteNonQuery();
        }
    }

    [Fact]
    public async Task GetAllPricesAsync_WithEmptyArray_ReturnsEmptyArray()
    {
        // Act
        var results = await _repository.GetPricesAndVatsAsync(Array.Empty<int>());

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetAllPricesAsync_WithMultiplePriceChanges_ReturnsLatestPrices()
    {
        // Arrange
        var insertItems = @"
            INSERT INTO Items (Id, Description) VALUES 
            (1, 'Test Item 1')";
        
        var insertVat = @"
            INSERT INTO Vat (ItemId, Rate, StartDate) VALUES 
            (1, 0.20, '2024-01-01'),
            (1, 0.25, '2024-02-01')";
        
        var insertPricing = @"
            INSERT INTO Pricing (ItemId, Price, StartDate) VALUES 
            (1, 100.00, '2024-01-01'),
            (1, 150.00, '2024-02-01'),
            (1, 120.00, '2024-03-01')";
        
        using var command = _connection.CreateCommand();
        command.CommandText = insertItems;
        command.ExecuteNonQuery();
        
        command.CommandText = insertVat;
        command.ExecuteNonQuery();
        
        command.CommandText = insertPricing;
        command.ExecuteNonQuery();

        try
        {
            var itemIds = new[] { 1 };

            // Act
            var results = await _repository.GetPricesAndVatsAsync(itemIds);

            // Assert
            Assert.Single(results);
            var item = results[0];
            Assert.Equal(1, item.Id);
            Assert.Equal(120.00m, item.Price);  // Should get the latest price from 2024-03-01
            Assert.Equal(0.25m, item.VatRate);  // Should get the latest VAT rate from 2024-02-01
        }
        finally
        {
            // Clean up test data
            var cleanup = @"
                DELETE FROM Pricing;
                DELETE FROM Vat;
                DELETE FROM Items;";
            
            command.CommandText = cleanup;
            command.ExecuteNonQuery();
        }
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
        
        // Clean up the database file
        if (File.Exists(_connectionString.Replace("Data Source=", "")))
        {
            File.Delete(_connectionString.Replace("Data Source=", ""));
        }
    }
} 