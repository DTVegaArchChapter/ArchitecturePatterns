using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Infrastructure.Repositories;

namespace RestaurantManagement.Api.IntegrationTests;

public sealed class MenuItemRepositoryTests : IDisposable
{
    private readonly TestDatabase _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetByIdAsync_SeededItem_ReturnsItem()
    {
        await using var context = _db.CreateContext();

        var item = await new MenuItemRepository(context).GetByIdAsync(1);

        Assert.NotNull(item);
        Assert.Equal("Margherita Pizza", item.Name);
        Assert.Equal(12.99m, item.Price);
    }

    [Fact]
    public async Task GetByIdsAsync_ReturnsOnlyRequestedItems()
    {
        await using var context = _db.CreateContext();

        var items = await new MenuItemRepository(context).GetByIdsAsync([1, 3, 999]);

        Assert.Equal([1, 3], items.Select(i => i.Id).Order());
    }

    [Fact]
    public async Task AddAsync_PersistsMenuItem()
    {
        await using (var context = _db.CreateContext())
        {
            await new MenuItemRepository(context).AddAsync(new MenuItem("Tiramisu", "Desserts", 7.5m, "Coffee dessert"));
            await context.SaveChangesAsync();
        }

        await using var verify = _db.CreateContext();
        var item = verify.MenuItems.Single(m => m.Name == "Tiramisu");
        Assert.Equal(7.5m, item.Price);
        Assert.True(item.IsAvailable);
    }

    [Fact]
    public async Task DeleteAsync_RemovesMenuItem()
    {
        await using (var context = _db.CreateContext())
        {
            var repository = new MenuItemRepository(context);
            var item = await repository.GetByIdAsync(8);
            await repository.DeleteAsync(item!);
            await context.SaveChangesAsync();
        }

        await using var verify = _db.CreateContext();
        Assert.Null(await new MenuItemRepository(verify).GetByIdAsync(8));
    }
}
