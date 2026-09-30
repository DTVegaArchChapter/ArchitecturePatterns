using FluentValidation;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.Tables.UpdateTableStatus;

public sealed class UpdateTableStatusCommandValidator : AbstractValidator<UpdateTableStatusCommand>
{
    public UpdateTableStatusCommandValidator()
    {
        RuleFor(x => x.TableId)
            .GreaterThan(0).WithMessage("TableId must be greater than 0");

        RuleFor(x => x.NewStatus)
            .Must(s => Enum.TryParse<TableStatus>(s, true, out var status) && Enum.IsDefined(status))
            .WithMessage("Invalid table status value");
    }
}
