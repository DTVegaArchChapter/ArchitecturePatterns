using RestaurantManagement.Application.Common.DTOs;

namespace RestaurantManagement.Application.Common.Interfaces;

/// <summary>
/// Read-only query side. Returns DTOs directly and never loads aggregates.
/// </summary>
public interface IOrderReadService
{
    Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetKitchenOrdersAsync(CancellationToken cancellationToken = default);
}
