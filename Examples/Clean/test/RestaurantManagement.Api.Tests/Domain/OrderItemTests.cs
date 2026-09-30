using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Api.Tests.Domain;

public class OrderItemTests
{
    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void Constructor_InvalidArguments_Throws(int quantity, decimal price) =>
        Assert.Throws<ArgumentException>(() => new OrderItem(1, quantity, price));

    [Fact]
    public void GetTotalPrice_MultipliesPriceByQuantity() =>
        Assert.Equal(30m, new OrderItem(1, 3, 10m).GetTotalPrice());

    [Fact]
    public void UpdateQuantity_Valid_ChangesQuantity()
    {
        var item = new OrderItem(1, 1, 10m);

        item.UpdateQuantity(4);

        Assert.Equal(4, item.Quantity);
    }

    [Fact]
    public void UpdateQuantity_NonPositive_Throws() =>
        Assert.Throws<ArgumentException>(() => new OrderItem(1, 1, 10m).UpdateQuantity(0));

    [Fact]
    public void UpdateSpecialInstructions_ChangesValue()
    {
        var item = new OrderItem(1, 1, 10m);

        item.UpdateSpecialInstructions("extra spicy");

        Assert.Equal("extra spicy", item.SpecialInstructions);
    }
}
