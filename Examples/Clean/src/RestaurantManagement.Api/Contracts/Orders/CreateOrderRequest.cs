namespace RestaurantManagement.Api.Contracts.Orders;

public record OrderItemRequest(int MenuItemId, int Quantity, string? SpecialInstructions);

public record CreateOrderRequest(int TableId, List<OrderItemRequest> Items, string? Notes);
