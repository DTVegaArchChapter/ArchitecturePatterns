using Mediator;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Api.Common;
using RestaurantManagement.Api.Contracts.Tables;
using RestaurantManagement.Application.Tables.GetAllTables;
using RestaurantManagement.Application.Tables.UpdateTableStatus;

namespace RestaurantManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TablesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IResult> GetAllTables(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllTablesQuery(), cancellationToken);
        return result.ToApiResult();
    }

    [HttpPut("{tableId:int}/status")]
    public async Task<IResult> UpdateTableStatus(
        int tableId,
        [FromBody] UpdateTableStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateTableStatusCommand(tableId, request.NewStatus), cancellationToken);
        return result.ToApiResult();
    }
}
