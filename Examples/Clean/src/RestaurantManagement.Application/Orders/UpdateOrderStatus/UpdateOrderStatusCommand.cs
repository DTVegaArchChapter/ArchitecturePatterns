using Mediator;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;

namespace RestaurantManagement.Application.Orders.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand(int OrderId, string NewStatus) : ICommand<Result<OrderDto>>;
