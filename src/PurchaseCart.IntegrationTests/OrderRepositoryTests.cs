using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using PurchaseCart.DataAccessSqlite;
using PurchaseCart.Domain;
using Xunit;
using Dapper;

namespace PurchaseCart.IntegrationTests;

public class OrderRepositoryTests : IDisposable
{
    private readonly string _connectionString;
    private readonly SqliteConnection _connection;
    private readonly OrderRepository _repository;
    private readonly ILogger<DBSchemaInitializer> _logger;

    public OrderRepositoryTests()
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
        
        _repository = new OrderRepository(_connectionString);
    }

    [Fact]
    public async Task Save_WithSingleItem_SavesOrderAndItemCorrectly()
    {
        // Arrange
        // Insert test item
        var insertItem = @"
            INSERT INTO Items (Id, Description) VALUES 
            (1, 'Test Item 1')";
        
        using var command = _connection.CreateCommand();
        command.CommandText = insertItem;
        command.ExecuteNonQuery();

        try
        {
            var order = new Order
            {
                Total = 120.00m,
                TotalVat = 20.00m,
                Date = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new()
                    {
                        ItemId = 1,
                        Quantity = 2,
                        Price = 50.00m,
                        VatValue = 10.00m
                    }
                }
            };

            // Act
            var savedOrder = await _repository.Save(order);

            // Assert
            Assert.NotNull(savedOrder.Id);
            
            // Verify order was saved correctly
            var orderQuery = "SELECT * FROM Orders WHERE Id = @Id";
            var savedOrderData = await _connection.QuerySingleAsync(orderQuery, new { Id = savedOrder.Id });
            Assert.Equal(order.Total, savedOrderData.Total);
            Assert.Equal(order.TotalVat, savedOrderData.TotalVat);

            // Verify order item was saved correctly
            var itemQuery = "SELECT * FROM OrderItems WHERE OrderId = @OrderId";
            var savedItem = await _connection.QuerySingleAsync(itemQuery, new { OrderId = savedOrder.Id });
            Assert.Equal(order.Items[0].ItemId, savedItem.ItemId);
            Assert.Equal(order.Items[0].Quantity, savedItem.Quantity);
            Assert.Equal(order.Items[0].Price, savedItem.Price);
            Assert.Equal(order.Items[0].VatValue, savedItem.VatValue);
        }
        finally
        {
            // Clean up test data
            var cleanup = @"
                DELETE FROM OrderItems;
                DELETE FROM Orders;
                DELETE FROM Items;";
            
            command.CommandText = cleanup;
            command.ExecuteNonQuery();
        }
    }

    [Fact]
    public async Task Save_WithMultipleItems_SavesAllItemsCorrectly()
    {
        // Arrange
        // Insert test items
        var insertItems = @"
            INSERT INTO Items (Id, Description) VALUES 
            (1, 'Test Item 1'),
            (2, 'Test Item 2')";
        
        using var command = _connection.CreateCommand();
        command.CommandText = insertItems;
        command.ExecuteNonQuery();

        try
        {
            var order = new Order
            {
                Total = 360.00m,
                TotalVat = 60.00m,
                Date = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new()
                    {
                        ItemId = 1,
                        Quantity = 2,
                        Price = 100.00m,
                        VatValue = 20.00m
                    },
                    new()
                    {
                        ItemId = 2,
                        Quantity = 1,
                        Price = 160.00m,
                        VatValue = 40.00m
                    }
                }
            };

            // Act
            var savedOrder = await _repository.Save(order);

            // Assert
            Assert.NotNull(savedOrder.Id);

            // Verify order was saved correctly
            var orderQuery = "SELECT * FROM Orders WHERE Id = @Id";
            var savedOrderData = await _connection.QuerySingleAsync(orderQuery, new { Id = savedOrder.Id });
            Assert.Equal(order.Total, savedOrderData.Total);
            Assert.Equal(order.TotalVat, savedOrderData.TotalVat);

            // Verify all order items were saved correctly
            var itemsQuery = "SELECT * FROM OrderItems WHERE OrderId = @OrderId ORDER BY ItemId";
            var savedItems = (await _connection.QueryAsync(itemsQuery, new { OrderId = savedOrder.Id })).ToList();
            
            Assert.Equal(2, savedItems.Count);
            
            // Verify first item
            Assert.Equal(order.Items[0].ItemId, savedItems[0].ItemId);
            Assert.Equal(order.Items[0].Quantity, savedItems[0].Quantity);
            Assert.Equal(order.Items[0].Price, savedItems[0].Price);
            Assert.Equal(order.Items[0].VatValue, savedItems[0].VatValue);

            // Verify second item
            Assert.Equal(order.Items[1].ItemId, savedItems[1].ItemId);
            Assert.Equal(order.Items[1].Quantity, savedItems[1].Quantity);
            Assert.Equal(order.Items[1].Price, savedItems[1].Price);
            Assert.Equal(order.Items[1].VatValue, savedItems[1].VatValue);
        }
        finally
        {
            // Clean up test data
            var cleanup = @"
                DELETE FROM OrderItems;
                DELETE FROM Orders;
                DELETE FROM Items;";
            
            command.CommandText = cleanup;
            command.ExecuteNonQuery();
        }
    }

    [Fact]
    public async Task Save_WithNullOrder_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.Save(null!));
    }

    [Fact]
    public async Task Save_WithNullItems_ThrowsArgumentException()
    {
        // Arrange
        var order = new Order
        {
            Total = 100.00m,
            TotalVat = 20.00m,
            Date = DateTime.UtcNow,
            Items = null!
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.Save(order));
        Assert.Contains("Order must contain at least one item", exception.Message);
    }

    [Fact]
    public async Task Save_WithEmptyItems_ThrowsArgumentException()
    {
        // Arrange
        var order = new Order
        {
            Total = 100.00m,
            TotalVat = 20.00m,
            Date = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.Save(order));
        Assert.Contains("Order must contain at least one item", exception.Message);
    }

    [Theory]
    [InlineData(0, 1, 10.0, 2.0, "ItemId")]
    [InlineData(-1, 1, 10.0, 2.0, "ItemId")]
    [InlineData(1, 0, 10.0, 2.0, "Quantity")]
    [InlineData(1, -1, 10.0, 2.0, "Quantity")]
    [InlineData(1, 1, -10.0, 2.0, "Price")]
    [InlineData(1, 1, 10.0, -2.0, "VatValue")]
    public async Task Save_WithInvalidOrderItem_ThrowsArgumentException(
        int itemId, int quantity, double price, double vatValue, string expectedErrorField)
    {
        // Arrange
        var order = new Order
        {
            Total = 100.00m,
            TotalVat = 20.00m,
            Date = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new()
                {
                    ItemId = itemId,
                    Quantity = quantity,
                    Price = (decimal)price,
                    VatValue = (decimal)vatValue
                }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.Save(order));
        Assert.Contains(expectedErrorField, exception.Message);
    }

    [Theory]
    [InlineData(-100.0, 20.0, "Total")]
    [InlineData(100.0, -20.0, "TotalVat")]
    public async Task Save_WithInvalidOrderTotals_ThrowsArgumentException(
        double total, double totalVat, string expectedErrorField)
    {
        // Arrange
        var order = new Order
        {
            Total = (decimal)total,
            TotalVat = (decimal)totalVat,
            Date = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new()
                {
                    ItemId = 1,
                    Quantity = 1,
                    Price = 10.00m,
                    VatValue = 2.00m
                }
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.Save(order));
        Assert.Contains(expectedErrorField, exception.Message);
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
