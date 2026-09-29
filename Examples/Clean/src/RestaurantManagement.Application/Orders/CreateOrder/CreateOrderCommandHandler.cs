using Mediator;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.Orders.CreateOrder;

public sealed class CreateOrderCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreateOrderCommand, Result<OrderDto>>
{
    public async ValueTask<Result<OrderDto>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var table = await unitOfWork.Tables.GetByIdAsync(command.TableId, cancellationToken);
        if (table is null)
        {
            return Result<OrderDto>.NotFound($"Table {command.TableId} not found");
        }

        if (!table.IsAvailable)
        {
            return Result<OrderDto>.Failure($"Table {table.TableNumber} is not available for orders");
        }

        var menuItemIds = command.Items.Select(i => i.MenuItemId).ToList();
        var menuItems = await unitOfWork.MenuItems.GetByIdsAsync(menuItemIds, cancellationToken);
        var availableMenuItems = menuItems.Where(m => m.IsAvailable).ToList();

        var availableMenuItemIds = availableMenuItems.Select(m => m.Id).ToList();
        var unavailableMenuItemIds = menuItemIds.Where(id => !availableMenuItemIds.Contains(id)).ToList();

        if (unavailableMenuItemIds.Count != 0)
        {
            return Result<OrderDto>.Failure(
                $"The following menu items are not available: {string.Join(", ", unavailableMenuItemIds)}",
                errorDetails: new Dictionary<string, object> { ["UnavailableMenuItemIds"] = unavailableMenuItemIds });
        }

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        var order = new Order(orderNumber, command.TableId, command.Notes);

        foreach (var itemRequest in command.Items)
        {
            var menuItem = availableMenuItems.First(m => m.Id == itemRequest.MenuItemId);
            var added = order.AddOrderItem(itemRequest.MenuItemId, itemRequest.Quantity, menuItem.Price, itemRequest.SpecialInstructions);
            if (!added.IsSuccess)
            {
                return Result<OrderDto>.Conflict(added.Error!);
            }
        }

        var occupied = table.Occupy();
        if (!occupied.IsSuccess)
        {
            return Result<OrderDto>.Conflict(occupied.Error!);
        }

        await unitOfWork.Orders.AddAsync(order, cancellationToken);
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return Result<OrderDto>.Conflict(ex.Message);
        }

        var orderItemDtos = order.OrderItems.Select(oi =>
        {
            var menuItem = availableMenuItems.First(m => m.Id == oi.MenuItemId);
            return new OrderItemDto(oi.Id, menuItem.Name, oi.Quantity, oi.Price, oi.SpecialInstructions);
        }).ToList();

        var orderDto = new OrderDto(
            order.Id,
            order.OrderNumber,
            order.TableId,
            order.OrderDate,
            order.Status.ToString(),
            order.TotalAmount,
            order.Notes,
            orderItemDtos);

        return Result<OrderDto>.Success(orderDto);
    }
}
