using Mediator;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;

namespace RestaurantManagement.Application.Tables.GetAllTables;

public sealed class GetAllTablesQueryHandler(ITableReadService tableReadService)
    : IQueryHandler<GetAllTablesQuery, Result<List<TableDto>>>
{
    public async ValueTask<Result<List<TableDto>>> Handle(GetAllTablesQuery query, CancellationToken cancellationToken)
    {
        var tables = await tableReadService.GetAllAsync(cancellationToken);

        return Result<List<TableDto>>.Success([.. tables]);
    }
}
