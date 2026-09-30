using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Api.IntegrationTests;

public sealed class UnitOfWorkTests : IDisposable
{
    private readonly TestDatabase _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task SaveChangesAsync_PersistsChangesAcrossRepositories()
    {
        var uow = _db.CreateUnitOfWork();
        await uow.MenuItems.AddAsync(new MenuItem("Tiramisu", "Desserts", 7.5m));
        var table = await uow.Tables.GetByIdAsync(1);
        table!.Occupy();

        var saved = await uow.SaveChangesAsync();

        Assert.Equal(2, saved);
        await using var verify = _db.CreateContext();
        Assert.Single(verify.MenuItems.Where(m => m.Name == "Tiramisu"));
        Assert.Equal(TableStatus.Occupied, verify.Tables.Single(t => t.Id == 1).Status);
    }

    [Fact]
    public async Task CommitTransactionAsync_PersistsChanges()
    {
        var uow = _db.CreateUnitOfWork();
        await uow.BeginTransactionAsync();
        await uow.Tables.AddAsync(new Table(20, 4));
        await uow.SaveChangesAsync();
        await uow.CommitTransactionAsync();

        await using var verify = _db.CreateContext();
        Assert.Single(verify.Tables.Where(t => t.TableNumber == 20));
    }

    [Fact]
    public async Task RollbackTransactionAsync_DiscardsChanges()
    {
        var uow = _db.CreateUnitOfWork();
        await uow.BeginTransactionAsync();
        await uow.Tables.AddAsync(new Table(21, 4));
        await uow.SaveChangesAsync();
        await uow.RollbackTransactionAsync();

        await using var verify = _db.CreateContext();
        Assert.Empty(verify.Tables.Where(t => t.TableNumber == 21));
    }

    [Fact]
    public async Task SaveChangesAsync_ConcurrentTableUpdate_ThrowsInvalidOperation()
    {
        var first = _db.CreateUnitOfWork();
        var second = _db.CreateUnitOfWork();

        var tableInFirst = await first.Tables.GetByIdAsync(3);
        var tableInSecond = await second.Tables.GetByIdAsync(3);

        tableInFirst!.Reserve(DateTime.UtcNow.AddHours(1));
        await first.SaveChangesAsync();

        tableInSecond!.Occupy();
        await Assert.ThrowsAsync<InvalidOperationException>(() => second.SaveChangesAsync());
    }
}
