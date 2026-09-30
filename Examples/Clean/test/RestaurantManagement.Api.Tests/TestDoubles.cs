using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Common;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Api.Tests;

internal static class EntityExtensions
{
    public static T WithId<T>(this T entity, int id) where T : BaseEntity
    {
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(entity, id);
        return entity;
    }
}

internal sealed class FakeTableRepository(params Table[] tables) : ITableRepository
{
    public List<Table> Items { get; } = [.. tables];

    public Task<Table?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(t => t.Id == id));

    public Task<Table?> GetByTableNumberAsync(int tableNumber, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(t => t.TableNumber == tableNumber));

    public Task AddAsync(Table table, CancellationToken cancellationToken = default)
    {
        Items.Add(table);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Table table, CancellationToken cancellationToken = default)
    {
        Items.Remove(table);
        return Task.CompletedTask;
    }
}

internal sealed class FakeMenuItemRepository(params MenuItem[] menuItems) : IMenuItemRepository
{
    public List<MenuItem> Items { get; } = [.. menuItems];

    public Task<MenuItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(m => m.Id == id));

    public Task<IReadOnlyList<MenuItem>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var set = ids.ToHashSet();
        return Task.FromResult<IReadOnlyList<MenuItem>>(Items.Where(m => set.Contains(m.Id)).ToList());
    }

    public Task AddAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
    {
        Items.Add(menuItem);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
    {
        Items.Remove(menuItem);
        return Task.CompletedTask;
    }
}

internal sealed class FakeOrderRepository(params Order[] orders) : IOrderRepository
{
    public List<Order> Items { get; } = [.. orders];

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(o => o.Id == id));

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        Items.Add(order);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Order order, CancellationToken cancellationToken = default)
    {
        Items.Remove(order);
        return Task.CompletedTask;
    }
}

internal sealed class FakeUnitOfWork(
    FakeTableRepository? tables = null,
    FakeMenuItemRepository? menuItems = null,
    FakeOrderRepository? orders = null) : IUnitOfWork
{
    public FakeTableRepository FakeTables { get; } = tables ?? new();
    public FakeMenuItemRepository FakeMenuItems { get; } = menuItems ?? new();
    public FakeOrderRepository FakeOrders { get; } = orders ?? new();

    public Exception? SaveException { get; set; }
    public int SaveChangesCount { get; private set; }

    public ITableRepository Tables => FakeTables;
    public IMenuItemRepository MenuItems => FakeMenuItems;
    public IOrderRepository Orders => FakeOrders;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (SaveException is not null)
            throw SaveException;

        SaveChangesCount++;
        return Task.FromResult(1);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class FakeOrderReadService(OrderDto? order = null) : IOrderReadService
{
    public Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(order);

    public Task<IReadOnlyList<OrderDto>> GetKitchenOrdersAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<OrderDto>>(order is null ? [] : [order]);
}
