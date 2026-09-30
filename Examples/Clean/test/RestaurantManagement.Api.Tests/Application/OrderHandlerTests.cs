using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Orders.CreateOrder;
using RestaurantManagement.Application.Orders.UpdateOrderStatus;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Api.Tests.Application;

public class CreateOrderCommandHandlerTests
{
    private static MenuItem Menu(int id, decimal price = 10m, bool available = true)
    {
        var item = new MenuItem($"Item{id}", "Main", price).WithId(id);
        if (!available) item.MakeUnavailable();
        return item;
    }

    private static CreateOrderCommand Command(int tableId = 1, params OrderItemInput[] items) =>
        new(tableId, [.. items], "notes");

    [Fact]
    public async Task Handle_TableNotFound_ReturnsNotFound()
    {
        var uow = new FakeUnitOfWork();
        var handler = new CreateOrderCommandHandler(uow);

        var result = await handler.Handle(Command(1, new OrderItemInput(1, 1, null)), default);

        Assert.Equal(ResultType.NotFound, result.ResultType);
        Assert.Empty(uow.FakeOrders.Items);
    }

    [Fact]
    public async Task Handle_TableNotAvailable_ReturnsFailure()
    {
        var table = new Table(1, 4).WithId(1);
        table.Occupy();
        var uow = new FakeUnitOfWork(new FakeTableRepository(table), new FakeMenuItemRepository(Menu(1)));

        var result = await new CreateOrderCommandHandler(uow)
            .Handle(Command(1, new OrderItemInput(1, 1, null)), default);

        Assert.Equal(ResultType.Failure, result.ResultType);
        Assert.Empty(uow.FakeOrders.Items);
    }

    [Fact]
    public async Task Handle_UnavailableMenuItem_ReturnsFailureWithDetails()
    {
        var uow = new FakeUnitOfWork(
            new FakeTableRepository(new Table(1, 4).WithId(1)),
            new FakeMenuItemRepository(Menu(1), Menu(2, available: false)));

        var result = await new CreateOrderCommandHandler(uow).Handle(
            Command(1, new OrderItemInput(1, 1, null), new OrderItemInput(2, 1, null), new OrderItemInput(3, 1, null)),
            default);

        Assert.Equal(ResultType.Failure, result.ResultType);
        var ids = Assert.IsType<List<int>>(result.ErrorDetails["UnavailableMenuItemIds"]);
        Assert.Equal([2, 3], ids);
        Assert.Empty(uow.FakeOrders.Items);
    }

    [Fact]
    public async Task Handle_Valid_CreatesOrderOccupiesTableAndSaves()
    {
        var table = new Table(5, 4).WithId(1);
        var uow = new FakeUnitOfWork(
            new FakeTableRepository(table),
            new FakeMenuItemRepository(Menu(1, 10m), Menu(2, 4m)));

        var result = await new CreateOrderCommandHandler(uow).Handle(
            Command(1, new OrderItemInput(1, 2, "no salt"), new OrderItemInput(2, 3, null)),
            default);

        Assert.True(result.IsSuccess);
        var dto = result.Data!;
        Assert.Equal(32m, dto.TotalAmount);
        Assert.Equal(nameof(OrderStatus.Pending), dto.Status);
        Assert.Equal("notes", dto.Notes);
        Assert.Equal(2, dto.OrderItems.Count);
        Assert.Contains(dto.OrderItems, i => i.MenuItemName == "Item1" && i.SpecialInstructions == "no salt");
        Assert.StartsWith("ORD-", dto.OrderNumber);

        Assert.Single(uow.FakeOrders.Items);
        Assert.Equal(TableStatus.Occupied, table.Status);
        Assert.Equal(1, uow.SaveChangesCount);
    }

    [Fact]
    public async Task Handle_SaveThrowsInvalidOperation_ReturnsConflict()
    {
        var uow = new FakeUnitOfWork(
            new FakeTableRepository(new Table(1, 4).WithId(1)),
            new FakeMenuItemRepository(Menu(1)))
        {
            SaveException = new InvalidOperationException("concurrency")
        };

        var result = await new CreateOrderCommandHandler(uow)
            .Handle(Command(1, new OrderItemInput(1, 1, null)), default);

        Assert.Equal(ResultType.Conflict, result.ResultType);
        Assert.Equal("concurrency", result.ErrorMessage);
    }
}

public class UpdateOrderStatusCommandHandlerTests
{
    private static Order PendingOrderWithItem(int id = 1)
    {
        var order = new Order("ORD-1", 1).WithId(id);
        order.AddOrderItem(1, 1, 10m);
        return order;
    }

    private static OrderDto Dto(int id = 1) =>
        new(id, "ORD-1", 1, DateTime.UtcNow, "InPreparation", 10m, null, []);

    [Fact]
    public async Task Handle_OrderNotFound_ReturnsNotFound()
    {
        var handler = new UpdateOrderStatusCommandHandler(new FakeUnitOfWork(), new FakeOrderReadService());

        var result = await handler.Handle(new UpdateOrderStatusCommand(1, "Ready"), default);

        Assert.Equal(ResultType.NotFound, result.ResultType);
    }

    [Fact]
    public async Task Handle_ValidTransition_SavesAndReturnsDto()
    {
        var order = PendingOrderWithItem();
        var uow = new FakeUnitOfWork(orders: new FakeOrderRepository(order));
        var handler = new UpdateOrderStatusCommandHandler(uow, new FakeOrderReadService(Dto()));

        var result = await handler.Handle(new UpdateOrderStatusCommand(1, "inpreparation"), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.InPreparation, order.Status);
        Assert.Equal(1, uow.SaveChangesCount);
    }

    [Fact]
    public async Task Handle_InvalidTransition_ReturnsConflictWithoutSaving()
    {
        var order = PendingOrderWithItem();
        var uow = new FakeUnitOfWork(orders: new FakeOrderRepository(order));
        var handler = new UpdateOrderStatusCommandHandler(uow, new FakeOrderReadService(Dto()));

        var result = await handler.Handle(new UpdateOrderStatusCommand(1, "Served"), default);

        Assert.Equal(ResultType.Conflict, result.ResultType);
        Assert.Equal(0, uow.SaveChangesCount);
    }

    [Fact]
    public async Task Handle_Cancel_CancelsOrder()
    {
        var order = PendingOrderWithItem();
        var uow = new FakeUnitOfWork(orders: new FakeOrderRepository(order));
        var handler = new UpdateOrderStatusCommandHandler(uow, new FakeOrderReadService(Dto()));

        var result = await handler.Handle(new UpdateOrderStatusCommand(1, "Cancelled"), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task Handle_PendingTarget_ReturnsFailure()
    {
        var uow = new FakeUnitOfWork(orders: new FakeOrderRepository(PendingOrderWithItem()));
        var handler = new UpdateOrderStatusCommandHandler(uow, new FakeOrderReadService(Dto()));

        var result = await handler.Handle(new UpdateOrderStatusCommand(1, "Pending"), default);

        Assert.Equal(ResultType.Failure, result.ResultType);
    }

    [Fact]
    public async Task Handle_ReadModelMissing_ReturnsNotFound()
    {
        var uow = new FakeUnitOfWork(orders: new FakeOrderRepository(PendingOrderWithItem()));
        var handler = new UpdateOrderStatusCommandHandler(uow, new FakeOrderReadService(null));

        var result = await handler.Handle(new UpdateOrderStatusCommand(1, "InPreparation"), default);

        Assert.Equal(ResultType.NotFound, result.ResultType);
    }
}
