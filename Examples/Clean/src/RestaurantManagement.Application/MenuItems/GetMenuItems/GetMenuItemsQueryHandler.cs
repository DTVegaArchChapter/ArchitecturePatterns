using Mediator;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;

namespace RestaurantManagement.Application.MenuItems.GetMenuItems;

public sealed class GetMenuItemsQueryHandler(IMenuItemReadService menuItemReadService)
    : IQueryHandler<GetMenuItemsQuery, Result<List<MenuItemDto>>>
{
    public async ValueTask<Result<List<MenuItemDto>>> Handle(GetMenuItemsQuery query, CancellationToken cancellationToken)
    {
        var menuItems = await menuItemReadService.GetAvailableAsync(cancellationToken);

        return Result<List<MenuItemDto>>.Success([.. menuItems]);
    }
}
