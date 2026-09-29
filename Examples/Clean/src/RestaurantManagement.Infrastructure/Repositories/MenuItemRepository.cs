using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Infrastructure.Data;

namespace RestaurantManagement.Infrastructure.Repositories;

public sealed class MenuItemRepository(RestaurantDbContext context) : IMenuItemRepository
{
    public async Task<MenuItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await context.MenuItems.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<MenuItem>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await context.MenuItems.Where(m => idList.Contains(m.Id)).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
    {
        await context.MenuItems.AddAsync(menuItem, cancellationToken);
    }

    public Task UpdateAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
    {
        context.MenuItems.Update(menuItem);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(MenuItem menuItem, CancellationToken cancellationToken = default)
    {
        context.MenuItems.Remove(menuItem);
        return Task.CompletedTask;
    }
}
