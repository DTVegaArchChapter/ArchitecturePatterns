using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.Orders.UpdateOrderStatus;

public sealed record UpdateOrderStatusRequest(int OrderId, OrderStatus NewStatus);
