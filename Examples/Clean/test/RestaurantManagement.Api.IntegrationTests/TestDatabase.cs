using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Infrastructure.Data;
using RestaurantManagement.Infrastructure.Repositories;

namespace RestaurantManagement.Api.IntegrationTests;

/// <summary>
/// Isolated shared-cache in-memory SQLite database (seeded by EF) so EF Core and Dapper connections see the same data.
/// </summary>
public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _keepAliveConnection;
    private readonly List<RestaurantDbContext> _unitOfWorkContexts = [];

    public string ConnectionString { get; }

    public TestDatabase()
    {
        ConnectionString = $"Data Source=file:{Guid.NewGuid():N}?mode=memory&cache=shared";
        _keepAliveConnection = new SqliteConnection(ConnectionString);
        _keepAliveConnection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public RestaurantDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<RestaurantDbContext>().UseSqlite(ConnectionString).Options);

    public IDbConnectionFactory CreateConnectionFactory() => new SqliteConnectionFactory(ConnectionString);

    public UnitOfWork CreateUnitOfWork()
    {
        var context = CreateContext();
        _unitOfWorkContexts.Add(context);
        return new UnitOfWork(
            context,
            new TableRepository(context),
            new MenuItemRepository(context),
            new OrderRepository(context));
    }

    public async Task<int> SeedOrderAsync(int tableId, params (int MenuItemId, int Quantity, decimal Price)[] items)
    {
        await using var context = CreateContext();

        var order = new Order($"ORD-{Guid.NewGuid():N}"[..20], tableId);
        foreach (var (menuItemId, quantity, price) in items)
        {
            order.AddOrderItem(menuItemId, quantity, price);
        }

        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return order.Id;
    }

    public void Dispose()
    {
        _unitOfWorkContexts.ForEach(c => c.Dispose());
        _keepAliveConnection.Dispose();
    }
}
