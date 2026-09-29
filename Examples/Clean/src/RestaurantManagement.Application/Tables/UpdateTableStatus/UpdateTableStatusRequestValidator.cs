using FluentValidation;

namespace RestaurantManagement.Application.Tables.UpdateTableStatus;

public sealed class UpdateTableStatusRequestValidator : AbstractValidator<UpdateTableStatusRequest>
{
    public UpdateTableStatusRequestValidator()
    {
        RuleFor(x => x.TableId)
            .GreaterThan(0).WithMessage("TableId must be greater than 0");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid table status value");
    }
}
