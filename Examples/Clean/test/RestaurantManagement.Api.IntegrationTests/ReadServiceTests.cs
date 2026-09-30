using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Infrastructure.ReadServices;

namespace RestaurantManagement.Api.IntegrationTests;

public sealed class ReadServiceTests : IDisposable
{
    private readonly TestDatabase _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task TableReadService_GetAllAsync_ReturnsSeededTablesOrderedByNumber()
    {
        var tables = await new DapperTableReadService(_db.CreateConnectionFactory()).GetAllAsync();

        Assert.Equal([1, 2, 3, 4, 5], tables.Select(t => t.TableNumber));
        Assert.All(tables, t => Assert.Equal("Available", t.Status));
    }

    [Fact]
    public async Task TableReadService_GetAllAsync_ReflectsWrittenState()
    {
        var reservedAt = new DateTime(2030, 1, 1, 19, 0, 0, DateTimeKind.Utc);
        await using (var context = _db.CreateContext())
        {
            context.Tables.Single(t => t.TableNumber == 2).Reserve(reservedAt);
            await context.SaveChangesAsync();
        }

        var tables = await new DapperTableReadService(_db.CreateConnectionFactory()).GetAllAsync();

        var table = tables.Single(t => t.TableNumber == 2);
        Assert.Equal("Reserved", table.Status);
        Assert.Equal(reservedAt, table.ReservedAt);
    }

    [Fact]
    public async Task MenuItemReadService_GetAvailableAsync_ReturnsAvailableItemsOrdered()
    {
        await using (var context = _db.CreateContext())
        {
            context.MenuItems.Single(m => m.Name == "Coffee").MakeUnavailable();
            await context.SaveChangesAsync();
        }

        var items = await new DapperMenuItemReadService(_db.CreateConnectionFactory()).GetAvailableAsync();

        Assert.Equal(7, items.Count);
        Assert.DoesNotContain(items, i => i.Name == "Coffee");
        Assert.All(items, i => Assert.True(i.IsAvailable));
        Assert.Equal(
            items.OrderBy(i => i.Category, StringComparer.Ordinal).ThenBy(i => i.Name, StringComparer.Ordinal),
            items);
    }

    [Fact]
    public async Task OrderReadService_GetByIdAsync_ReturnsOrderWithItems()
    {
        var orderId = await _db.SeedOrderAsync(3, (1, 2, 12.99m), (7, 1, 2.99m));

        var order = await new DapperOrderReadService(_db.CreateConnectionFactory()).GetByIdAsync(orderId);

        Assert.NotNull(order);
        Assert.Equal(3, order.TableId);
        Assert.Equal("Pending", order.Status);
        Assert.Equal(28.97m, order.TotalAmount);
        Assert.Equal(2, order.OrderItems.Count);
        Assert.Contains(order.OrderItems, i => i.MenuItemName == "Margherita Pizza" && i.Quantity == 2 && i.Price == 12.99m);
        Assert.Contains(order.OrderItems, i => i.MenuItemName == "Coffee" && i.Quantity == 1);
    }

    [Fact]
    public async Task OrderReadService_GetByIdAsync_UnknownId_ReturnsNull()
    {
        var order = await new DapperOrderReadService(_db.CreateConnectionFactory()).GetByIdAsync(999);

        Assert.Null(order);
    }

    [Fact]
    public async Task OrderReadService_GetKitchenOrdersAsync_ReturnsOnlyPendingAndInPreparation()
    {
        var pendingId = await _db.SeedOrderAsync(1, (1, 1, 12.99m));
        var inPreparationId = await _db.SeedOrderAsync(2, (2, 1, 14.99m));
        var readyId = await _db.SeedOrderAsync(3, (3, 1, 8.99m));
        var cancelledId = await _db.SeedOrderAsync(4, (4, 1, 18.99m));

        await using (var context = _db.CreateContext())
        {
            context.Orders.Include(o => o.OrderItems).Single(o => o.Id == inPreparationId).StartPreparation();
            var ready = context.Orders.Include(o => o.OrderItems).Single(o => o.Id == readyId);
            ready.StartPreparation();
            ready.MarkAsReady();
            context.Orders.Single(o => o.Id == cancelledId).Cancel();
            await context.SaveChangesAsync();
        }

        var orders = await new DapperOrderReadService(_db.CreateConnectionFactory()).GetKitchenOrdersAsync();

        Assert.Equal([pendingId, inPreparationId], orders.Select(o => o.Id));
        Assert.All(orders, o => Assert.NotEmpty(o.OrderItems));
    }
}
