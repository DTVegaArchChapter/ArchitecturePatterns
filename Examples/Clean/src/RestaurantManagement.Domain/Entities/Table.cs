using RestaurantManagement.Domain.Common;

namespace RestaurantManagement.Domain.Entities;

public class Table : BaseEntity, IAggregateRoot
{
    public int TableNumber { get; private set; }
    public int Capacity { get; private set; }
    public TableStatus Status { get; private set; }
    public DateTime? ReservedAt { get; private set; }

    private Table() { } // For EF Core

    public Table(int tableNumber, int capacity)
    {
        if (tableNumber <= 0)
            throw new ArgumentException("Table number must be positive", nameof(tableNumber));
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be positive", nameof(capacity));

        TableNumber = tableNumber;
        Capacity = capacity;
        Status = TableStatus.Available;
    }

    public bool IsAvailable => Status == TableStatus.Available;

    public DomainResult Reserve(DateTime reservationTime)
    {
        if (Status != TableStatus.Available)
            return DomainResult.Failure($"Cannot reserve table {TableNumber}. Current status: {Status}");

        Status = TableStatus.Reserved;
        ReservedAt = reservationTime;
        return DomainResult.Success();
    }

    public DomainResult Occupy()
    {
        if (Status != TableStatus.Available && Status != TableStatus.Reserved)
            return DomainResult.Failure($"Cannot occupy table {TableNumber}. Current status: {Status}");

        Status = TableStatus.Occupied;
        ReservedAt = null;
        return DomainResult.Success();
    }

    public DomainResult MakeAvailable()
    {
        if (Status == TableStatus.Available)
            return DomainResult.Failure($"Table {TableNumber} is already available");

        Status = TableStatus.Available;
        ReservedAt = null;
        return DomainResult.Success();
    }

    public DomainResult TakeOutOfService()
    {
        if (Status == TableStatus.Occupied)
            return DomainResult.Failure($"Cannot take occupied table {TableNumber} out of service");

        Status = TableStatus.OutOfService;
        ReservedAt = null;
        return DomainResult.Success();
    }
}
