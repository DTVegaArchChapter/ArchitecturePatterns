using FluentValidation.TestHelper;
using RestaurantManagement.Application.Orders.CreateOrder;
using RestaurantManagement.Application.Orders.UpdateOrderStatus;
using RestaurantManagement.Application.Tables.UpdateTableStatus;

namespace RestaurantManagement.Api.Tests.Application;

public class ValidatorTests
{
    private static OrderItemInput Item(int id = 1, int qty = 1, string? instructions = null) => new(id, qty, instructions);

    [Fact]
    public void CreateOrder_Valid_HasNoErrors()
    {
        var command = new CreateOrderCommand(1, [Item(1), Item(2)], "ok");

        new CreateOrderCommandValidator().TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateOrder_InvalidTableId_HasError()
    {
        var command = new CreateOrderCommand(0, [Item()], null);

        new CreateOrderCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.TableId);
    }

    [Fact]
    public void CreateOrder_NoItems_HasError()
    {
        var command = new CreateOrderCommand(1, [], null);

        new CreateOrderCommandValidator().TestValidate(command).ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void CreateOrder_DuplicateItems_HasError()
    {
        var command = new CreateOrderCommand(1, [Item(1), Item(1)], null);

        new CreateOrderCommandValidator().TestValidate(command)
            .ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("Order cannot contain duplicate menu items");
    }

    [Fact]
    public void CreateOrder_InvalidItem_HasError()
    {
        var command = new CreateOrderCommand(1, [Item(0, 0)], null);

        var result = new CreateOrderCommandValidator().TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].MenuItemId");
        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Fact]
    public void CreateOrder_TooLongInstructionsAndNotes_HaveErrors()
    {
        var command = new CreateOrderCommand(1, [Item(1, 1, new string('x', 251))], new string('y', 501));

        var result = new CreateOrderCommandValidator().TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].SpecialInstructions");
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    [Theory]
    [InlineData(1, "Ready", true)]
    [InlineData(1, "ready", true)]
    [InlineData(0, "Ready", false)]
    [InlineData(1, "Bogus", false)]
    [InlineData(1, "99", false)]
    public void UpdateOrderStatus_Validates(int id, string status, bool valid)
    {
        var result = new UpdateOrderStatusCommandValidator().Validate(new UpdateOrderStatusCommand(id, status));

        Assert.Equal(valid, result.IsValid);
    }

    [Theory]
    [InlineData(1, "Occupied", true)]
    [InlineData(1, "outofservice", true)]
    [InlineData(0, "Occupied", false)]
    [InlineData(1, "Bogus", false)]
    [InlineData(1, "99", false)]
    public void UpdateTableStatus_Validates(int id, string status, bool valid)
    {
        var result = new UpdateTableStatusCommandValidator().Validate(new UpdateTableStatusCommand(id, status));

        Assert.Equal(valid, result.IsValid);
    }
}
