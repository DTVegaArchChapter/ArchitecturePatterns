namespace RestaurantManagement.Application.Orders.CreateOrder;

public sealed record OrderItemRequest(int MenuItemId, int Quantity, string? SpecialInstructions);

public sealed record CreateOrderRequest(int TableId, List<OrderItemRequest> Items, string? Notes);
