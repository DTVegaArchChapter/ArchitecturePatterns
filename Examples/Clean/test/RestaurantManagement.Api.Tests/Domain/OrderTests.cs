using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Api.Tests.Domain;

public class OrderTests
{
    private static Order NewOrder() => new("ORD-1", 1);

    private static Order OrderWithItem()
    {
        var order = NewOrder();
        order.AddOrderItem(1, 2, 10m);
        return order;
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_BlankOrderNumber_Throws(string number) =>
        Assert.Throws<ArgumentException>(() => new Order(number, 1));

    [Fact]
    public void Constructor_StartsPendingWithZeroTotal()
    {
        var order = new Order("ORD-1", 3, "no onions");

        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(0m, order.TotalAmount);
        Assert.Equal(3, order.TableId);
        Assert.Equal("no onions", order.Notes);
        Assert.True(order.CanBeModified);
    }

    [Fact]
    public void AddOrderItem_UpdatesTotal()
    {
        var order = NewOrder();

        order.AddOrderItem(1, 2, 10m);
        var result = order.AddOrderItem(2, 1, 5m);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, order.OrderItems.Count);
        Assert.Equal(25m, order.TotalAmount);
    }

    [Fact]
    public void AddOrderItem_Duplicate_Fails()
    {
        var order = OrderWithItem();

        var result = order.AddOrderItem(1, 1, 10m);

        Assert.False(result.IsSuccess);
        Assert.Single(order.OrderItems);
    }

    [Fact]
    public void AddOrderItem_WhenNotPending_Fails()
    {
        var order = OrderWithItem();
        order.StartPreparation();

        Assert.False(order.AddOrderItem(2, 1, 5m).IsSuccess);
    }

    [Fact]
    public void RemoveOrderItem_RecalculatesTotal()
    {
        var order = OrderWithItem();
        order.AddOrderItem(2, 1, 5m);

        var result = order.RemoveOrderItem(1);

        Assert.True(result.IsSuccess);
        Assert.Equal(5m, order.TotalAmount);
    }

    [Fact]
    public void RemoveOrderItem_WhenNotPending_Fails()
    {
        var order = OrderWithItem();
        order.StartPreparation();

        Assert.False(order.RemoveOrderItem(1).IsSuccess);
    }

    [Fact]
    public void UpdateOrderItemQuantity_RecalculatesTotal()
    {
        var order = OrderWithItem();

        var result = order.UpdateOrderItemQuantity(1, 5);

        Assert.True(result.IsSuccess);
        Assert.Equal(50m, order.TotalAmount);
    }

    [Fact]
    public void UpdateOrderItemQuantity_WhenNotPending_Fails()
    {
        var order = OrderWithItem();
        order.StartPreparation();

        Assert.False(order.UpdateOrderItemQuantity(1, 5).IsSuccess);
    }

    [Fact]
    public void StartPreparation_WithoutItems_Fails()
    {
        var order = NewOrder();

        Assert.False(order.StartPreparation().IsSuccess);
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void FullLifecycle_MovesThroughStatuses()
    {
        var order = OrderWithItem();

        Assert.True(order.StartPreparation().IsSuccess);
        Assert.Equal(OrderStatus.InPreparation, order.Status);
        Assert.False(order.CanBeModified);

        Assert.True(order.MarkAsReady().IsSuccess);
        Assert.Equal(OrderStatus.Ready, order.Status);

        Assert.True(order.Serve().IsSuccess);
        Assert.Equal(OrderStatus.Served, order.Status);
    }

    [Fact]
    public void StartPreparation_WhenNotPending_Fails()
    {
        var order = OrderWithItem();
        order.StartPreparation();

        Assert.False(order.StartPreparation().IsSuccess);
    }

    [Fact]
    public void MarkAsReady_WhenPending_Fails() =>
        Assert.False(OrderWithItem().MarkAsReady().IsSuccess);

    [Fact]
    public void Serve_WhenNotReady_Fails() =>
        Assert.False(OrderWithItem().Serve().IsSuccess);

    [Fact]
    public void Cancel_WhenPending_Succeeds()
    {
        var order = OrderWithItem();

        Assert.True(order.Cancel().IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_Fails()
    {
        var order = OrderWithItem();
        order.Cancel();

        Assert.False(order.Cancel().IsSuccess);
    }

    [Fact]
    public void Cancel_WhenServed_Fails()
    {
        var order = OrderWithItem();
        order.StartPreparation();
        order.MarkAsReady();
        order.Serve();

        Assert.False(order.Cancel().IsSuccess);
        Assert.Equal(OrderStatus.Served, order.Status);
    }

    [Fact]
    public void UpdateNotes_ChangesNotes()
    {
        var order = NewOrder();

        order.UpdateNotes("allergy");

        Assert.Equal("allergy", order.Notes);
    }
}
