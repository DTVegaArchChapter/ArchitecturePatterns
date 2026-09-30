using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Infrastructure.Repositories;

namespace RestaurantManagement.Api.IntegrationTests;

public sealed class TableRepositoryTests : IDisposable
{
    private readonly TestDatabase _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetByIdAsync_SeededTable_ReturnsTable()
    {
        await using var context = _db.CreateContext();

        var table = await new TableRepository(context).GetByIdAsync(1);

        Assert.NotNull(table);
        Assert.Equal(1, table.TableNumber);
        Assert.Equal(4, table.Capacity);
        Assert.Equal(TableStatus.Available, table.Status);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var context = _db.CreateContext();

        Assert.Null(await new TableRepository(context).GetByIdAsync(999));
    }

    [Fact]
    public async Task GetByTableNumberAsync_ReturnsMatchingTable()
    {
        await using var context = _db.CreateContext();

        var table = await new TableRepository(context).GetByTableNumberAsync(5);

        Assert.NotNull(table);
        Assert.Equal(8, table.Capacity);
    }

    [Fact]
    public async Task AddAsync_PersistsTable()
    {
        await using (var context = _db.CreateContext())
        {
            await new TableRepository(context).AddAsync(new Table(10, 2));
            await context.SaveChangesAsync();
        }

        await using var verify = _db.CreateContext();
        var table = await new TableRepository(verify).GetByTableNumberAsync(10);
        Assert.NotNull(table);
        Assert.Equal(2, table.Capacity);
    }

    [Fact]
    public async Task AddAsync_DuplicateTableNumber_Throws()
    {
        await using var context = _db.CreateContext();
        await new TableRepository(context).AddAsync(new Table(1, 2));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task DeleteAsync_RemovesTable()
    {
        await using (var context = _db.CreateContext())
        {
            var repository = new TableRepository(context);
            var table = await repository.GetByIdAsync(4);
            await repository.DeleteAsync(table!);
            await context.SaveChangesAsync();
        }

        await using var verify = _db.CreateContext();
        Assert.Null(await new TableRepository(verify).GetByIdAsync(4));
    }

    [Fact]
    public async Task StateChanges_ArePersisted()
    {
        var reservedAt = new DateTime(2030, 1, 1, 19, 0, 0, DateTimeKind.Utc);

        await using (var context = _db.CreateContext())
        {
            var table = await new TableRepository(context).GetByIdAsync(2);
            Assert.True(table!.Reserve(reservedAt).IsSuccess);
            await context.SaveChangesAsync();
        }

        await using var verify = _db.CreateContext();
        var reloaded = await new TableRepository(verify).GetByIdAsync(2);
        Assert.Equal(TableStatus.Reserved, reloaded!.Status);
        Assert.Equal(reservedAt, reloaded.ReservedAt);
    }
}
