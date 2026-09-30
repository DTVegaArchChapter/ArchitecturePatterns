using FluentValidation;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.Behaviors;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Tables.UpdateTableStatus;

namespace RestaurantManagement.Api.Tests.Application;

public class ValidationBehaviorTests
{
    private sealed class AlwaysFailsValidator : AbstractValidator<UpdateTableStatusCommand>
    {
        public AlwaysFailsValidator() =>
            RuleFor(x => x.TableId).Must(_ => false).WithMessage("bad id");
    }

    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<UpdateTableStatusCommand, Result<TableDto>>([]);
        var called = false;

        var result = await behavior.Handle(
            new UpdateTableStatusCommand(1, "Occupied"),
            (_, _) =>
            {
                called = true;
                return ValueTask.FromResult(Result<TableDto>.Success(new TableDto(1, 1, 4, "Occupied", null)));
            },
            default);

        Assert.True(called);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ValidationFails_ShortCircuitsWithFailure()
    {
        var behavior = new ValidationBehavior<UpdateTableStatusCommand, Result<TableDto>>([new AlwaysFailsValidator()]);
        var called = false;

        var result = await behavior.Handle(
            new UpdateTableStatusCommand(1, "Occupied"),
            (_, _) =>
            {
                called = true;
                return ValueTask.FromResult(Result<TableDto>.Success(new TableDto(1, 1, 4, "Occupied", null)));
            },
            default);

        Assert.False(called);
        Assert.Equal(ResultType.Failure, result.ResultType);
        Assert.Contains("bad id", result.ErrorMessage);
        Assert.True(result.ErrorDetails.ContainsKey(nameof(UpdateTableStatusCommand.TableId)));
    }
}
