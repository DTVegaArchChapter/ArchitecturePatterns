using FluentValidation;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.Orders.UpdateOrderStatus;

public sealed class UpdateOrderStatusUseCase(
    IUnitOfWork unitOfWork,
    IValidator<UpdateOrderStatusRequest> validator)
{
    public async Task<Result<OrderDto>> ExecuteAsync(UpdateOrderStatusRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            var propertyErrors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result<OrderDto>.ValidationFailure(errors, propertyErrors);
        }

        var order = await unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result<OrderDto>.NotFound($"Order {request.OrderId} not found");
        }

        try
        {
            switch (request.NewStatus)
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
                    return Result<OrderDto>.Failure($"Cannot transition order to status: {request.NewStatus}");
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result<OrderDto>.Conflict(ex.Message);
        }

        await unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var orderItemDtos = order.OrderItems.Select(oi => new OrderItemDto(
            oi.Id,
            oi.MenuItem?.Name ?? string.Empty,
            oi.Quantity,
            oi.Price,
            oi.SpecialInstructions)).ToList();

        var orderDto = new OrderDto(
            order.Id,
            order.OrderNumber,
            order.TableId,
            order.OrderDate,
            order.Status.ToString(),
            order.TotalAmount,
            order.Notes,
            orderItemDtos);

        return Result<OrderDto>.Success(orderDto);
    }
}
