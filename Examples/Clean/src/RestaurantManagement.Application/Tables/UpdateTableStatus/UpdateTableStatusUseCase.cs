using FluentValidation;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.Tables.UpdateTableStatus;

public sealed class UpdateTableStatusUseCase(
    IUnitOfWork unitOfWork,
    IValidator<UpdateTableStatusRequest> validator)
{
    public async Task<Result<TableDto>> ExecuteAsync(UpdateTableStatusRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            var propertyErrors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result<TableDto>.ValidationFailure(errors, propertyErrors);
        }

        var table = await unitOfWork.Tables.GetByIdAsync(request.TableId, cancellationToken);
        if (table is null)
        {
            return Result<TableDto>.NotFound($"Table {request.TableId} not found");
        }

        try
        {
            switch (request.NewStatus)
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
