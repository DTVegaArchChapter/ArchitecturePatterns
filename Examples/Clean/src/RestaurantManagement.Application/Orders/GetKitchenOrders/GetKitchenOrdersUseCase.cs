using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;

namespace RestaurantManagement.Application.Orders.GetKitchenOrders;

public sealed class GetKitchenOrdersUseCase(IUnitOfWork unitOfWork)
{
    public async Task<Result<List<OrderDto>>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var orders = await unitOfWork.Orders.GetKitchenOrdersAsync(cancellationToken);

        if (!orders.Any())
        {
            return Result<List<OrderDto>>.Success([]);
        }

        var orderDtos = orders.Select(order =>
        {
            var orderItemDtos = order.OrderItems.Select(oi => new OrderItemDto(
                oi.Id,
                oi.MenuItem?.Name ?? string.Empty,
                oi.Quantity,
                oi.Price,
                oi.SpecialInstructions)).ToList();

            return new OrderDto(
                order.Id,
                order.OrderNumber,
                order.TableId,
                order.OrderDate,
                order.Status.ToString(),
                order.TotalAmount,
                order.Notes,
                orderItemDtos);
        }).ToList();

        return Result<List<OrderDto>>.Success(orderDtos);
    }
}
