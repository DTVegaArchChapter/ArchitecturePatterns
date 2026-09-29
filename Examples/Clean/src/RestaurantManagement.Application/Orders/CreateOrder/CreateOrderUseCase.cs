using FluentValidation;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Application.Orders.CreateOrder;

public sealed class CreateOrderUseCase(
    IUnitOfWork unitOfWork,
    IValidator<CreateOrderRequest> validator)
{
    public async Task<Result<OrderDto>> ExecuteAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            var propertyErrors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Result<OrderDto>.ValidationFailure(errors, propertyErrors);
        }

        var table = await unitOfWork.Tables.GetByIdAsync(request.TableId, cancellationToken);
        if (table is null)
        {
            return Result<OrderDto>.NotFound($"Table {request.TableId} not found");
        }

        if (!table.IsAvailable)
        {
            return Result<OrderDto>.Failure($"Table {table.TableNumber} is not available for orders");
        }

        var menuItemIds = request.Items.Select(i => i.MenuItemId).ToList();
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

        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        var order = new Order(orderNumber, request.TableId, request.Notes);

        foreach (var itemRequest in request.Items)
        {
            var menuItem = availableMenuItems.First(m => m.Id == itemRequest.MenuItemId);
            order.AddOrderItem(itemRequest.MenuItemId, itemRequest.Quantity, menuItem.Price, itemRequest.SpecialInstructions);
        }

        await unitOfWork.Orders.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

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
