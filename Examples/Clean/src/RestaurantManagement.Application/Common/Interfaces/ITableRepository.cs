using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.Common.Interfaces;

public interface ITableRepository
{
    Task<Table?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Table?> GetByTableNumberAsync(int tableNumber, CancellationToken cancellationToken = default);
    Task AddAsync(Table table, CancellationToken cancellationToken = default);
    Task DeleteAsync(Table table, CancellationToken cancellationToken = default);
}
