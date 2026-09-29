using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.Tables.UpdateTableStatus;

public sealed record UpdateTableStatusRequest(int TableId, TableStatus NewStatus);
