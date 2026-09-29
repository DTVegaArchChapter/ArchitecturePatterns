using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Api.Common;
using RestaurantManagement.Api.Contracts.Orders;
using RestaurantManagement.Domain.Entities;
using CreateOrderUseCase = RestaurantManagement.Application.Orders.CreateOrder.CreateOrderUseCase;
using UpdateOrderStatusUseCase = RestaurantManagement.Application.Orders.UpdateOrderStatus.UpdateOrderStatusUseCase;
using GetKitchenOrdersUseCase = RestaurantManagement.Application.Orders.GetKitchenOrders.GetKitchenOrdersUseCase;
using AppCreateOrderRequest = RestaurantManagement.Application.Orders.CreateOrder.CreateOrderRequest;
using AppOrderItemRequest = RestaurantManagement.Application.Orders.CreateOrder.OrderItemRequest;
using AppUpdateOrderStatusRequest = RestaurantManagement.Application.Orders.UpdateOrderStatus.UpdateOrderStatusRequest;

namespace RestaurantManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(
    CreateOrderUseCase createOrderUseCase,
    UpdateOrderStatusUseCase updateOrderStatusUseCase,
    GetKitchenOrdersUseCase getKitchenOrdersUseCase) : ControllerBase
{
    [HttpPost]
    public async Task<IResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var orderItems = request.Items
            .Select(i => new AppOrderItemRequest(i.MenuItemId, i.Quantity, i.SpecialInstructions))
            .ToList();

        var useCaseRequest = new AppCreateOrderRequest(request.TableId, orderItems, request.Notes);
        var result = await createOrderUseCase.ExecuteAsync(useCaseRequest, cancellationToken);

        return result.ToApiResult(data => Results.Created($"/api/orders/{data?.Id}", data));
    }

    [HttpPut("{orderId:int}/status")]
    public async Task<IResult> UpdateOrderStatus(
        int orderId,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<OrderStatus>(request.NewStatus, true, out var status))
        {
            return Results.BadRequest("Invalid order status");
        }

        var useCaseRequest = new AppUpdateOrderStatusRequest(orderId, status);
        var result = await updateOrderStatusUseCase.ExecuteAsync(useCaseRequest, cancellationToken);
        return result.ToApiResult();
    }

    [HttpGet("kitchen")]
    public async Task<IResult> GetKitchenOrders(CancellationToken cancellationToken)
    {
        var result = await getKitchenOrdersUseCase.ExecuteAsync(cancellationToken);
        return result.ToApiResult();
    }
}
