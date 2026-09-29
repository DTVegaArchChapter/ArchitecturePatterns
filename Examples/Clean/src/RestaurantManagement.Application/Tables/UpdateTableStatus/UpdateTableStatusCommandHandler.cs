using Mediator;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;
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

        try
        {
            switch (Enum.Parse<TableStatus>(command.NewStatus, true))
            {
                case TableStatus.Available:
                    table.MakeAvailable();
                    break;
                case TableStatus.Occupied:
                    table.Occupy();
                    break;
                case TableStatus.Reserved:
                    table.Reserve(DateTime.UtcNow);
                    break;
                case TableStatus.OutOfService:
                    table.TakeOutOfService();
                    break;
            }
        }
        catch (InvalidOperationException ex)
        {
            return Result<TableDto>.Conflict(ex.Message);
        }

        await unitOfWork.Tables.UpdateAsync(table, cancellationToken);
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
