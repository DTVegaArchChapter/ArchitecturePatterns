using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Api.Common;
using RestaurantManagement.Application.MenuItems.GetMenuItems;

namespace RestaurantManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuItemsController(GetMenuItemsUseCase getMenuItemsUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<IResult> GetMenuItems(CancellationToken cancellationToken)
    {
        var result = await getMenuItemsUseCase.ExecuteAsync(cancellationToken);
        return result.ToApiResult();
    }
}
