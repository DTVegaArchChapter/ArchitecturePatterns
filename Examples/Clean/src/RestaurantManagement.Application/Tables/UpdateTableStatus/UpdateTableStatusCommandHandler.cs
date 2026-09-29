using Mediator;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Common;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.Tables.UpdateTableStatus;

public sealed class UpdateTableStatusCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTableStatusCommand, Result<TableDto>>
{
    public async ValueTask<Result<TableDto>> Handle(UpdateTableStatusCommand command, CancellationToken cancellationToken)
    {
        var table = await unitOfWork.Tables.GetByIdAsync(command.TableId, cancellationToken);
        if (table is null)
        {
            return Result<TableDto>.NotFound($"Table {command.TableId} not found");
        }

        var transition = Enum.Parse<TableStatus>(command.NewStatus, true) switch
        {
            TableStatus.Available => table.MakeAvailable(),
            TableStatus.Occupied => table.Occupy(),
            TableStatus.Reserved => table.Reserve(DateTime.UtcNow),
            TableStatus.OutOfService => table.TakeOutOfService(),
            _ => DomainResult.Success()
        };

        if (!transition.IsSuccess)
        {
            return Result<TableDto>.Conflict(transition.Error!);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var tableDto = new TableDto(
            table.Id,
            table.TableNumber,
            table.Capacity,
            table.Status.ToString(),
            table.ReservedAt);

        return Result<TableDto>.Success(tableDto);
    }
}
