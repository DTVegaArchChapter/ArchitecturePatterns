using Mediator;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;

namespace RestaurantManagement.Application.Tables.UpdateTableStatus;

public sealed record UpdateTableStatusCommand(int TableId, string NewStatus) : ICommand<Result<TableDto>>;
