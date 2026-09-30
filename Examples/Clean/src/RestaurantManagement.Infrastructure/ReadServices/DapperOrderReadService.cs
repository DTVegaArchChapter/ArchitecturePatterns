using Dapper;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Infrastructure.Data;

namespace RestaurantManagement.Infrastructure.ReadServices;

public sealed class DapperOrderReadService(IDbConnectionFactory connectionFactory) : IOrderReadService
{
    private const string OrderColumns =
        "o.Id, o.OrderNumber, o.TableId, o.OrderDate, o.Status, o.TotalAmount, o.Notes";

    public async Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var orders = await QueryAsync(
            $"SELECT {OrderColumns} FROM Orders o WHERE o.Id = @Id",
            new { Id = id },
            cancellationToken);

        return orders.FirstOrDefault();
    }

    public Task<IReadOnlyList<OrderDto>> GetKitchenOrdersAsync(CancellationToken cancellationToken = default)
    {
        return QueryAsync(
            $"SELECT {OrderColumns} FROM Orders o WHERE o.Status IN (@Pending, @InPreparation) ORDER BY o.OrderDate",
            new { Pending = (int)OrderStatus.Pending, InPreparation = (int)OrderStatus.InPreparation },
            cancellationToken);
    }

    private async Task<IReadOnlyList<OrderDto>> QueryAsync(string ordersSql, object parameters, CancellationToken cancellationToken)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var orderRows = (await connection.QueryAsync<OrderRow>(
            new CommandDefinition(ordersSql, parameters, cancellationToken: cancellationToken))).ToList();

        if (orderRows.Count == 0)
        {
            return [];
        }

        const string itemsSql = """
            SELECT oi.Id, oi.OrderId, mi.Name AS MenuItemName, oi.Quantity, oi.Price, oi.SpecialInstructions
            FROM OrderItems oi
            INNER JOIN MenuItems mi ON mi.Id = oi.MenuItemId
            WHERE oi.OrderId IN @OrderIds
            ORDER BY oi.Id
            """;

        var itemRows = await connection.QueryAsync<OrderItemRow>(
            new CommandDefinition(
                itemsSql,
                new { OrderIds = orderRows.Select(o => o.Id).ToArray() },
                cancellationToken: cancellationToken));

        var itemsByOrder = itemRows
            .GroupBy(i => i.OrderId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(i => new OrderItemDto(i.Id, i.MenuItemName, i.Quantity, i.Price, i.SpecialInstructions)).ToList());

        return orderRows
            .Select(o => new OrderDto(
                o.Id,
                o.OrderNumber,
                o.TableId,
                o.OrderDate,
                ((OrderStatus)o.Status).ToString(),
                o.TotalAmount,
                o.Notes,
                itemsByOrder.GetValueOrDefault(o.Id, [])))
            .ToList();
    }

    private sealed class OrderRow
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int TableId { get; set; }
        public DateTime OrderDate { get; set; }
        public int Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
    }

    private sealed class OrderItemRow
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string MenuItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? SpecialInstructions { get; set; }
    }
}
