using RestaurantManagement.Application.Common.DTOs;

namespace RestaurantManagement.Application.Common.Interfaces;

public interface ITableReadService
{
    Task<IReadOnlyList<TableDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
