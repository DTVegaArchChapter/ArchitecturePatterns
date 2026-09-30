using Mediator;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;

namespace RestaurantManagement.Application.MenuItems.GetMenuItems;

public sealed record GetMenuItemsQuery : IQuery<Result<List<MenuItemDto>>>;
