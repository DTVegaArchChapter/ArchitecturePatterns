using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;

namespace RestaurantManagement.Application.MenuItems.GetMenuItems;

public sealed class GetMenuItemsUseCase(IUnitOfWork unitOfWork)
{
    public async Task<Result<List<MenuItemDto>>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var menuItems = await unitOfWork.MenuItems.GetAvailableAsync(cancellationToken);

        var menuItemDtos = menuItems
            .Select(m => new MenuItemDto(
                m.Id,
                m.Name,
                m.Category,
                m.Price,
                m.Description,
                m.IsAvailable))
            .ToList();

        return Result<List<MenuItemDto>>.Success(menuItemDtos);
    }
}
