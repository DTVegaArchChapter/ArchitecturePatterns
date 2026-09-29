using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.Common.Interfaces;

public interface IMenuItemRepository
{
    Task<MenuItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MenuItem>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    Task AddAsync(MenuItem menuItem, CancellationToken cancellationToken = default);
    Task DeleteAsync(MenuItem menuItem, CancellationToken cancellationToken = default);
}
