using Mediator;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.Orders.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler(
    IUnitOfWork unitOfWork,
    IOrderReadService orderReadService)
    : ICommandHandler<UpdateOrderStatusCommand, Result<OrderDto>>
{
    public async ValueTask<Result<OrderDto>> Handle(UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Orders.GetByIdAsync(command.OrderId, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.NotFound($"Order {command.OrderId} not found");
        }

        try
        {
            var newStatus = Enum.Parse<OrderStatus>(command.NewStatus, true);
            switch (newStatus)
            {
                case OrderStatus.InPreparation:
                    order.StartPreparation();
                    break;
                case OrderStatus.Ready:
                    order.MarkAsReady();
                    break;
                case OrderStatus.Served:
                    order.Serve();
                    break;
                case OrderStatus.Cancelled:
                    order.Cancel();
                    break;
                default:
                    return Result<OrderDto>.Failure($"Cannot transition order to status: {newStatus}");
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result<OrderDto>.Conflict(ex.Message);
        }

        await unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var orderDto = await orderReadService.GetByIdAsync(order.Id, cancellationToken);

        return orderDto is null
            ? Result<OrderDto>.NotFound($"Order {command.OrderId} not found")
            : Result<OrderDto>.Success(orderDto);
    }
}
