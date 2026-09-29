using Mediator;
using RestaurantManagement.Api.Common;
using RestaurantManagement.Api.Contracts.Tables;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Tables.GetAllTables;
using RestaurantManagement.Application.Tables.UpdateTableStatus;

namespace RestaurantManagement.Api.Endpoints;

public static class TableEndpoints
{
    public static IEndpointRouteBuilder MapTableEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/tables").WithTags("Tables");

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetAllTablesQuery(), ct);
                return result.ToApiResult();
            })
            .Produces<List<TableDto>>();

        group.MapPut("{tableId:int}/status", async (int tableId, UpdateTableStatusRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new UpdateTableStatusCommand(tableId, request.NewStatus), ct);
                return result.ToApiResult();
            })
            .Produces<TableDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
