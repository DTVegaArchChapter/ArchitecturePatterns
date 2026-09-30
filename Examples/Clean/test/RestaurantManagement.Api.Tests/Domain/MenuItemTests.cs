using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Api.Tests.Domain;

public class MenuItemTests
{
    [Theory]
    [InlineData("", "Main", 10)]
    [InlineData(" ", "Main", 10)]
    [InlineData("Pasta", "", 10)]
    [InlineData("Pasta", "Main", 0)]
    [InlineData("Pasta", "Main", -1)]
    public void Constructor_InvalidArguments_Throws(string name, string category, decimal price) =>
        Assert.Throws<ArgumentException>(() => new MenuItem(name, category, price));

    [Fact]
    public void Constructor_SetsValuesAndIsAvailable()
    {
        var item = new MenuItem("Pasta", "Main", 12.5m, "Fresh");

        Assert.Equal("Pasta", item.Name);
        Assert.Equal("Main", item.Category);
        Assert.Equal(12.5m, item.Price);
        Assert.Equal("Fresh", item.Description);
        Assert.True(item.IsAvailable);
    }

    [Fact]
    public void UpdatePrice_Valid_ChangesPrice()
    {
        var item = new MenuItem("Pasta", "Main", 12.5m);

        item.UpdatePrice(15m);

        Assert.Equal(15m, item.Price);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void UpdatePrice_NonPositive_Throws(decimal price) =>
        Assert.Throws<ArgumentException>(() => new MenuItem("Pasta", "Main", 10m).UpdatePrice(price));

    [Fact]
    public void Availability_CanBeToggled()
    {
        var item = new MenuItem("Pasta", "Main", 10m);

        item.MakeUnavailable();
        Assert.False(item.IsAvailable);

        item.MakeAvailable();
        Assert.True(item.IsAvailable);
    }
}
