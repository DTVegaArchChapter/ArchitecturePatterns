using FluentValidation;

namespace RestaurantManagement.Application.Orders.UpdateOrderStatus;

public sealed class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("OrderId must be greater than 0");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid order status value");
    }
}
