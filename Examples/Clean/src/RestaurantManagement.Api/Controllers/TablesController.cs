using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Api.Common;
using RestaurantManagement.Api.Contracts.Tables;
using RestaurantManagement.Domain.Entities;
using GetAllTablesUseCase = RestaurantManagement.Application.Tables.GetAllTables.GetAllTablesUseCase;
using UpdateTableStatusUseCase = RestaurantManagement.Application.Tables.UpdateTableStatus.UpdateTableStatusUseCase;
using AppUpdateTableStatusRequest = RestaurantManagement.Application.Tables.UpdateTableStatus.UpdateTableStatusRequest;

namespace RestaurantManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TablesController(
    GetAllTablesUseCase getAllTablesUseCase,
    UpdateTableStatusUseCase updateTableStatusUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<IResult> GetAllTables(CancellationToken cancellationToken)
    {
        var result = await getAllTablesUseCase.ExecuteAsync(cancellationToken);
        return result.ToApiResult();
    }

    [HttpPut("{tableId:int}/status")]
    public async Task<IResult> UpdateTableStatus(
        int tableId,
        [FromBody] UpdateTableStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TableStatus>(request.NewStatus, true, out var status))
        {
            return Results.BadRequest("Invalid table status");
        }

        var useCaseRequest = new AppUpdateTableStatusRequest(tableId, status);
        var result = await updateTableStatusUseCase.ExecuteAsync(useCaseRequest, cancellationToken);
        return result.ToApiResult();
    }
}
