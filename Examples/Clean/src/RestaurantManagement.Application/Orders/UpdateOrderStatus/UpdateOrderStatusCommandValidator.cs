using FluentValidation;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.Orders.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("OrderId must be greater than 0");

        RuleFor(x => x.NewStatus)
            .Must(s => Enum.TryParse<OrderStatus>(s, true, out var status) && Enum.IsDefined(status))
            .WithMessage("Invalid order status value");
    }
}
