using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;

namespace RestaurantManagement.Application.Tables.GetAllTables;

public sealed class GetAllTablesUseCase(IUnitOfWork unitOfWork)
{
    public async Task<Result<List<TableDto>>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var tables = await unitOfWork.Tables.GetAllAsync(cancellationToken);

        var tableDtos = tables
            .OrderBy(t => t.TableNumber)
            .Select(t => new TableDto(
                t.Id,
                t.TableNumber,
                t.Capacity,
                t.Status.ToString(),
                t.ReservedAt))
            .ToList();

        return Result<List<TableDto>>.Success(tableDtos);
    }
}
