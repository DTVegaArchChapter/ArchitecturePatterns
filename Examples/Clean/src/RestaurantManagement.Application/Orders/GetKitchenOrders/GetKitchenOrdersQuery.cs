using Mediator;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;

namespace RestaurantManagement.Application.Orders.GetKitchenOrders;

public sealed record GetKitchenOrdersQuery : IQuery<Result<List<OrderDto>>>;
