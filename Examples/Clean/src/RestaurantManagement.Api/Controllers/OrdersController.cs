using Mediator;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Api.Common;
using RestaurantManagement.Api.Contracts.Orders;
using RestaurantManagement.Application.Orders.CreateOrder;
using RestaurantManagement.Application.Orders.GetKitchenOrders;
using RestaurantManagement.Application.Orders.UpdateOrderStatus;

namespace RestaurantManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var orderItems = request.Items
            .Select(i => new RestaurantManagement.Application.Orders.CreateOrder.OrderItemRequest(i.MenuItemId, i.Quantity, i.SpecialInstructions))
            .ToList();

        var command = new CreateOrderCommand(request.TableId, orderItems, request.Notes);
        var result = await sender.Send(command, cancellationToken);

        return result.ToApiResult(data => Results.Created($"/api/orders/{data?.Id}", data));
    }

    [HttpPut("{orderId:int}/status")]
    public async Task<IResult> UpdateOrderStatus(
        int orderId,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateOrderStatusCommand(orderId, request.NewStatus), cancellationToken);
        return result.ToApiResult();
    }

    [HttpGet("kitchen")]
    public async Task<IResult> GetKitchenOrders(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetKitchenOrdersQuery(), cancellationToken);
        return result.ToApiResult();
    }
}
