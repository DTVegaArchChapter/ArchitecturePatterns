using Mediator;
using RestaurantManagement.Application.Common;
using RestaurantManagement.Application.Common.DTOs;
using RestaurantManagement.Application.Common.Interfaces;

namespace RestaurantManagement.Application.Orders.GetKitchenOrders;

public sealed class GetKitchenOrdersQueryHandler(IOrderReadService orderReadService)
    : IQueryHandler<GetKitchenOrdersQuery, Result<List<OrderDto>>>
{
    public async ValueTask<Result<List<OrderDto>>> Handle(GetKitchenOrdersQuery query, CancellationToken cancellationToken)
    {
        var orders = await orderReadService.GetKitchenOrdersAsync(cancellationToken);

        return Result<List<OrderDto>>.Success([.. orders]);
    }
}
