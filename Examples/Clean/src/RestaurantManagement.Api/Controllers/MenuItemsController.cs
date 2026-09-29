using Mediator;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Api.Common;
using RestaurantManagement.Application.MenuItems.GetMenuItems;

namespace RestaurantManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuItemsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IResult> GetMenuItems(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMenuItemsQuery(), cancellationToken);
        return result.ToApiResult();
    }
}
