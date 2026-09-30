using RestaurantManagement.Application.Common.DTOs;

namespace RestaurantManagement.Application.Common.Interfaces;

public interface IMenuItemReadService
{
    Task<IReadOnlyList<MenuItemDto>> GetAvailableAsync(CancellationToken cancellationToken = default);
}
