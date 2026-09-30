using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Infrastructure.Repositories;

namespace RestaurantManagement.Api.IntegrationTests;

public sealed class OrderRepositoryTests : IDisposable
{
    private readonly TestDatabase _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task AddAsync_PersistsOrderWithItems()
    {
        int orderId;
        await using (var context = _db.CreateContext())
        {
            var order = new Order("ORD-1", 1, "No onions");
            order.AddOrderItem(1, 2, 12.99m);
            order.AddOrderItem(7, 1, 2.99m, "Extra hot");

            await new OrderRepository(context).AddAsync(order);
            await context.SaveChangesAsync();
            orderId = order.Id;
        }

        await using var verify = _db.CreateContext();
        var loaded = await new OrderRepository(verify).GetByIdAsync(orderId);

        Assert.NotNull(loaded);
        Assert.Equal("ORD-1", loaded.OrderNumber);
        Assert.Equal("No onions", loaded.Notes);
        Assert.Equal(OrderStatus.Pending, loaded.Status);
        Assert.Equal(28.97m, loaded.TotalAmount);
        Assert.Equal(2, loaded.OrderItems.Count);
        Assert.Contains(loaded.OrderItems, i => i.MenuItemId == 7 && i.SpecialInstructions == "Extra hot");
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = _db.CreateContext();

        Assert.Null(await new OrderRepository(context).GetByIdAsync(999));
    }

    [Fact]
    public async Task StatusTransitions_ArePersisted()
    {
        var orderId = await _db.SeedOrderAsync(1, (1, 1, 12.99m));

        await using (var context = _db.CreateContext())
        {
            var order = await new OrderRepository(context).GetByIdAsync(orderId);
            Assert.True(order!.StartPreparation().IsSuccess);
            await context.SaveChangesAsync();
        }

        await using var verify = _db.CreateContext();
        var reloaded = await new OrderRepository(verify).GetByIdAsync(orderId);
        Assert.Equal(OrderStatus.InPreparation, reloaded!.Status);
    }

    [Fact]
    public async Task ItemChanges_ArePersistedAndTotalRecalculated()
    {
        var orderId = await _db.SeedOrderAsync(1, (1, 1, 12.99m), (7, 1, 2.99m));

        await using (var context = _db.CreateContext())
        {
            var order = await new OrderRepository(context).GetByIdAsync(orderId);
            order!.RemoveOrderItem(7);
            order.UpdateOrderItemQuantity(1, 3);
            await context.SaveChangesAsync();
        }

        await using var verify = _db.CreateContext();
        var reloaded = await new OrderRepository(verify).GetByIdAsync(orderId);
        var item = Assert.Single(reloaded!.OrderItems);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(38.97m, reloaded.TotalAmount);
    }

    [Fact]
    public async Task DeleteAsync_CascadesToOrderItems()
    {
        var orderId = await _db.SeedOrderAsync(1, (1, 1, 12.99m), (7, 1, 2.99m));

        await using (var context = _db.CreateContext())
        {
            var repository = new OrderRepository(context);
            var order = await repository.GetByIdAsync(orderId);
            await repository.DeleteAsync(order!);
            await context.SaveChangesAsync();
        }

        await using var verify = _db.CreateContext();
        Assert.Null(await new OrderRepository(verify).GetByIdAsync(orderId));
        Assert.Empty(verify.OrderItems.Where(i => i.OrderId == orderId));
    }

    [Fact]
    public async Task AddAsync_DuplicateOrderNumber_Throws()
    {
        await _db.SeedOrderAsync(1, (1, 1, 12.99m));
        await using var seedContext = _db.CreateContext();
        var existingNumber = seedContext.Orders.Select(o => o.OrderNumber).First();

        await using var context = _db.CreateContext();
        await new OrderRepository(context).AddAsync(new Order(existingNumber, 2));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task AddAsync_UnknownTable_Throws()
    {
        await using var context = _db.CreateContext();
        await new OrderRepository(context).AddAsync(new Order("ORD-FK", 999));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }
}
