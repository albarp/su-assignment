using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using PurchaseCart.Domain.Entities;
using PurchaseCart.Domain.Interfaces;

namespace PurchaseCart.DataAccessSqlite;

public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Order> Save(Order order)
    {
        ValidateOrder(order);

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var orderId = await InsertOrder(connection, order, transaction);
            await InsertOrderItems(connection, orderId, order.Items, transaction);
            await transaction.CommitAsync();

            order.Id = orderId;
            return order;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static void ValidateOrder(Order order)
    {
        if (order == null)
        {
            throw new ArgumentNullException(nameof(order), "Order cannot be null");
        }

        ValidateOrderItems(order);
        ValidateOrderTotals(order);
    }

    private static void ValidateOrderItems(Order order)
    {
        if (order.Items == null || !order.Items.Any())
        {
            throw new ArgumentException("Order must contain at least one item", nameof(order));
        }

        foreach (var item in order.Items)
        {
            ValidateOrderItem(item);
        }
    }

    private static void ValidateOrderItem(OrderItem item)
    {
        if (item.ItemId <= 0)
        {
            throw new ArgumentException($"Invalid ItemId: {item.ItemId}. ItemId must be greater than 0", nameof(item));
        }
        if (item.Quantity <= 0)
        {
            throw new ArgumentException($"Invalid Quantity: {item.Quantity}. Quantity must be greater than 0", nameof(item));
        }
        if (item.Price < 0)
        {
            throw new ArgumentException($"Invalid Price: {item.Price}. Price cannot be negative", nameof(item));
        }
        if (item.VatValue < 0)
        {
            throw new ArgumentException($"Invalid VatValue: {item.VatValue}. VatValue cannot be negative", nameof(item));
        }
    }

    private static void ValidateOrderTotals(Order order)
    {
        if (order.Total < 0)
        {
            throw new ArgumentException($"Invalid Total: {order.Total}. Total cannot be negative", nameof(order));
        }
        if (order.TotalVat < 0)
        {
            throw new ArgumentException($"Invalid TotalVat: {order.TotalVat}. TotalVat cannot be negative", nameof(order));
        }
    }

    private static async Task<int> InsertOrder(SqliteConnection connection, Order order, IDbTransaction transaction)
    {
        const string insertOrderSql = @"
            INSERT INTO Orders (Total, TotalVat, Date)
            VALUES (@Total, @TotalVat, @Date);
            SELECT last_insert_rowid();";

        return await connection.ExecuteScalarAsync<int>(
            insertOrderSql,
            new { order.Total, order.TotalVat, Date = DateTime.UtcNow },
            transaction
        );
    }

    private static async Task InsertOrderItems(SqliteConnection connection, int orderId, List<OrderItem> items, IDbTransaction transaction)
    {
        const string insertOrderItemSql = @"
            INSERT INTO OrderItems (OrderId, ItemId, Quantity, Price, VatValue)
            VALUES (@OrderId, @ItemId, @Quantity, @Price, @VatValue);";

        foreach (var item in items)
        {
            await connection.ExecuteAsync(
                insertOrderItemSql,
                new
                {
                    OrderId = orderId,
                    item.ItemId,
                    item.Quantity,
                    item.Price,
                    item.VatValue
                },
                transaction
            );
        }
    }
}
