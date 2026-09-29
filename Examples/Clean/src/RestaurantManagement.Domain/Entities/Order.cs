using RestaurantManagement.Domain.Common;

namespace RestaurantManagement.Domain.Entities;

public class Order : BaseEntity, IAggregateRoot
{
    public string OrderNumber { get; private set; } = null!;
    public int TableId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? Notes { get; private set; }

    // Navigation properties
    private readonly List<OrderItem> _orderItems = [];
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    private Order() { } // For EF Core

    public Order(string orderNumber, int tableId, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number cannot be null or empty", nameof(orderNumber));

        OrderNumber = orderNumber;
        TableId = tableId;
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        Notes = notes;
        TotalAmount = 0;
    }

    public DomainResult AddOrderItem(int menuItemId, int quantity, decimal price, string? specialInstructions = null)
    {
        if (Status != OrderStatus.Pending)
            return DomainResult.Failure($"Cannot add items to order with status: {Status}");

        if (_orderItems.Any(oi => oi.MenuItemId == menuItemId))
            return DomainResult.Failure($"Menu item {menuItemId} is already in the order");

        var orderItem = new OrderItem(menuItemId, quantity, price, specialInstructions);
        _orderItems.Add(orderItem);
        RecalculateTotal();
        return DomainResult.Success();
    }

    public DomainResult RemoveOrderItem(int menuItemId)
    {
        if (Status != OrderStatus.Pending)
            return DomainResult.Failure($"Cannot remove items from order with status: {Status}");

        var item = _orderItems.FirstOrDefault(oi => oi.MenuItemId == menuItemId);
        if (item != null)
        {
            _orderItems.Remove(item);
            RecalculateTotal();
        }
        return DomainResult.Success();
    }

    public DomainResult UpdateOrderItemQuantity(int menuItemId, int newQuantity)
    {
        if (Status != OrderStatus.Pending)
            return DomainResult.Failure($"Cannot update items in order with status: {Status}");

        var item = _orderItems.FirstOrDefault(oi => oi.MenuItemId == menuItemId);
        if (item != null)
        {
            item.UpdateQuantity(newQuantity);
            RecalculateTotal();
        }
        return DomainResult.Success();
    }

    public DomainResult StartPreparation()
    {
        if (Status != OrderStatus.Pending)
            return DomainResult.Failure($"Cannot start preparation for order with status: {Status}");

        if (!_orderItems.Any())
            return DomainResult.Failure("Cannot start preparation for order with no items");

        Status = OrderStatus.InPreparation;
        return DomainResult.Success();
    }

    public DomainResult MarkAsReady()
    {
        if (Status != OrderStatus.InPreparation)
            return DomainResult.Failure($"Cannot mark order as ready with status: {Status}");

        Status = OrderStatus.Ready;
        return DomainResult.Success();
    }

    public DomainResult Serve()
    {
        if (Status != OrderStatus.Ready)
            return DomainResult.Failure($"Cannot serve order with status: {Status}");

        Status = OrderStatus.Served;
        return DomainResult.Success();
    }

    public DomainResult Cancel()
    {
        if (Status == OrderStatus.Served)
            return DomainResult.Failure("Cannot cancel a served order");
        if (Status == OrderStatus.Cancelled)
            return DomainResult.Failure("Order is already cancelled");

        Status = OrderStatus.Cancelled;
        return DomainResult.Success();
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
    }

    private void RecalculateTotal()
    {
        TotalAmount = _orderItems.Sum(item => item.GetTotalPrice());
    }

    public bool CanBeModified => Status == OrderStatus.Pending;
}
