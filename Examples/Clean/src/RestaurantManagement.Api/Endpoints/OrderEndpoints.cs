using Mediator;
using RestaurantManagement.Api.Common;
using RestaurantManagement.Api.Contracts.Orders;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Orders.CreateOrder;
using RestaurantManagement.Application.Orders.GetKitchenOrders;
using RestaurantManagement.Application.Orders.UpdateOrderStatus;

namespace RestaurantManagement.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/orders").WithTags("Orders");

        group.MapPost("/", async (CreateOrderRequest request, ISender sender, CancellationToken ct) =>
            {
                var items = request.Items
                    .Select(i => new OrderItemInput(i.MenuItemId, i.Quantity, i.SpecialInstructions))
                    .ToList();

                var result = await sender.Send(new CreateOrderCommand(request.TableId, items, request.Notes), ct);
                return result.ToApiResult(data => Results.Created($"/api/orders/{data?.Id}", data));
            })
            .Produces<OrderDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("{orderId:int}/status", async (int orderId, UpdateOrderStatusRequest request, ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new UpdateOrderStatusCommand(orderId, request.NewStatus), ct);
                return result.ToApiResult();
            })
            .Produces<OrderDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("kitchen", async (ISender sender, CancellationToken ct) =>
            {
                var result = await sender.Send(new GetKitchenOrdersQuery(), ct);
                return result.ToApiResult();
            })
            .Produces<List<OrderDto>>();

        return app;
    }
}
