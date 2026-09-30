using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Tables.UpdateTableStatus;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Api.Tests.Application;

public class UpdateTableStatusCommandHandlerTests
{
    [Fact]
    public async Task Handle_TableNotFound_ReturnsNotFound()
    {
        var handler = new UpdateTableStatusCommandHandler(new FakeUnitOfWork());

        var result = await handler.Handle(new UpdateTableStatusCommand(1, "Occupied"), default);

        Assert.Equal(ResultType.NotFound, result.ResultType);
    }

    [Theory]
    [InlineData("Occupied", TableStatus.Occupied)]
    [InlineData("reserved", TableStatus.Reserved)]
    [InlineData("OutOfService", TableStatus.OutOfService)]
    public async Task Handle_ValidTransition_UpdatesSavesAndReturnsDto(string status, TableStatus expected)
    {
        var table = new Table(7, 4).WithId(1);
        var uow = new FakeUnitOfWork(new FakeTableRepository(table));

        var result = await new UpdateTableStatusCommandHandler(uow)
            .Handle(new UpdateTableStatusCommand(1, status), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, table.Status);
        Assert.Equal(expected.ToString(), result.Data!.Status);
        Assert.Equal(7, result.Data.TableNumber);
        Assert.Equal(1, uow.SaveChangesCount);
    }

    [Fact]
    public async Task Handle_InvalidTransition_ReturnsConflictWithoutSaving()
    {
        var table = new Table(1, 4).WithId(1);
        var uow = new FakeUnitOfWork(new FakeTableRepository(table));

        var result = await new UpdateTableStatusCommandHandler(uow)
            .Handle(new UpdateTableStatusCommand(1, "Available"), default);

        Assert.Equal(ResultType.Conflict, result.ResultType);
        Assert.Equal(0, uow.SaveChangesCount);
    }
}
