using FluentValidation;

namespace RestaurantManagement.Application.Orders.CreateOrder;

public sealed class OrderItemRequestValidator : AbstractValidator<OrderItemRequest>
{
    public OrderItemRequestValidator()
    {
        RuleFor(x => x.MenuItemId)
            .GreaterThan(0).WithMessage("MenuItemId must be greater than 0");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.SpecialInstructions)
            .MaximumLength(250).WithMessage("Special instructions cannot exceed 250 characters");
    }
}

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.TableId)
            .GreaterThan(0).WithMessage("TableId must be greater than 0");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item");

        RuleFor(x => x.Items)
            .Must(items => items is null || items.Select(i => i.MenuItemId).Distinct().Count() == items.Count)
            .WithMessage("Order cannot contain duplicate menu items");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemRequestValidator());

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters");
    }
}
