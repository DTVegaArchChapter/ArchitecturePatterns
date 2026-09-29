using Mediator;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;

namespace RestaurantManagement.Application.Orders.CreateOrder;

public sealed record OrderItemRequest(int MenuItemId, int Quantity, string? SpecialInstructions);

public sealed record CreateOrderCommand(int TableId, List<OrderItemRequest> Items, string? Notes)
    : ICommand<Result<OrderDto>>;
