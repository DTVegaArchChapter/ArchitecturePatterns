namespace RestaurantManagement.Application.Common.Interfaces;

public interface IUnitOfWork
{
    ITableRepository Tables { get; }
    IMenuItemRepository MenuItems { get; }
    IOrderRepository Orders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
