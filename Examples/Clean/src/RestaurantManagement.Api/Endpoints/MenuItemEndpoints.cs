using Mediator;
using RestaurantManagement.Api.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.MenuItems.GetMenuItems;

namespace RestaurantManagement.Api.Endpoints;

public static class MenuItemEndpoints
{
    public static IEndpointRouteBuilder MapMenuItemEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/menuitems", async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetMenuItemsQuery(), ct);
                return result.ToApiResult();
            })
            .WithTags("MenuItems")
            .Produces<List<MenuItemDto>>();

        return app;
    }
}
