using Mediator;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Common;
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

        var newStatus = Enum.Parse<OrderStatus>(command.NewStatus, true);
        DomainResult transition;
        switch (newStatus)
        {
            case OrderStatus.InPreparation:
                transition = order.StartPreparation();
                break;
            case OrderStatus.Ready:
                transition = order.MarkAsReady();
                break;
            case OrderStatus.Served:
                transition = order.Serve();
                break;
            case OrderStatus.Cancelled:
                transition = order.Cancel();
                break;
            default:
                return Result<OrderDto>.Failure($"Cannot transition order to status: {newStatus}");
        }

        if (!transition.IsSuccess)
        {
            return Result<OrderDto>.Conflict(transition.Error!);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var orderDto = await orderReadService.GetByIdAsync(order.Id, cancellationToken);

        return orderDto is null
            ? Result<OrderDto>.NotFound($"Order {command.OrderId} not found")
            : Result<OrderDto>.Success(orderDto);
    }
}
