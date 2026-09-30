using Mediator;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;

namespace RestaurantManagement.Application.Tables.GetAllTables;

public sealed record GetAllTablesQuery : IQuery<Result<List<TableDto>>>;
