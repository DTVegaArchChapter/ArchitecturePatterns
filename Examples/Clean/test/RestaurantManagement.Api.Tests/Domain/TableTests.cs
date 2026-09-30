using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Api.Tests.Domain;

public class TableTests
{
    [Theory]
    [InlineData(0, 4)]
    [InlineData(-1, 4)]
    [InlineData(1, 0)]
    [InlineData(1, -2)]
    public void Constructor_InvalidArguments_Throws(int number, int capacity) =>
        Assert.Throws<ArgumentException>(() => new Table(number, capacity));

    [Fact]
    public void Constructor_StartsAvailable()
    {
        var table = new Table(1, 4);

        Assert.Equal(TableStatus.Available, table.Status);
        Assert.True(table.IsAvailable);
        Assert.Null(table.ReservedAt);
    }

    [Fact]
    public void Reserve_WhenAvailable_SetsReservedAndTime()
    {
        var table = new Table(1, 4);
        var time = new DateTime(2026, 1, 1, 18, 0, 0, DateTimeKind.Utc);

        var result = table.Reserve(time);

        Assert.True(result.IsSuccess);
        Assert.Equal(TableStatus.Reserved, table.Status);
        Assert.Equal(time, table.ReservedAt);
    }

    [Fact]
    public void Reserve_WhenNotAvailable_Fails()
    {
        var table = new Table(1, 4);
        table.Occupy();

        var result = table.Reserve(DateTime.UtcNow);

        Assert.False(result.IsSuccess);
        Assert.Equal(TableStatus.Occupied, table.Status);
    }

    [Fact]
    public void Occupy_WhenReserved_ClearsReservation()
    {
        var table = new Table(1, 4);
        table.Reserve(DateTime.UtcNow);

        var result = table.Occupy();

        Assert.True(result.IsSuccess);
        Assert.Equal(TableStatus.Occupied, table.Status);
        Assert.Null(table.ReservedAt);
    }

    [Fact]
    public void Occupy_WhenOutOfService_Fails()
    {
        var table = new Table(1, 4);
        table.TakeOutOfService();

        Assert.False(table.Occupy().IsSuccess);
    }

    [Fact]
    public void MakeAvailable_WhenAlreadyAvailable_Fails() =>
        Assert.False(new Table(1, 4).MakeAvailable().IsSuccess);

    [Fact]
    public void MakeAvailable_WhenOccupied_Succeeds()
    {
        var table = new Table(1, 4);
        table.Occupy();

        var result = table.MakeAvailable();

        Assert.True(result.IsSuccess);
        Assert.True(table.IsAvailable);
    }

    [Fact]
    public void TakeOutOfService_WhenOccupied_Fails()
    {
        var table = new Table(1, 4);
        table.Occupy();

        Assert.False(table.TakeOutOfService().IsSuccess);
    }

    [Fact]
    public void TakeOutOfService_WhenReserved_ClearsReservation()
    {
        var table = new Table(1, 4);
        table.Reserve(DateTime.UtcNow);

        var result = table.TakeOutOfService();

        Assert.True(result.IsSuccess);
        Assert.Equal(TableStatus.OutOfService, table.Status);
        Assert.Null(table.ReservedAt);
    }
}
