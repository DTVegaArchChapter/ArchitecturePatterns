namespace RestaurantManagement.Domain.Common;

public readonly record struct DomainResult(bool IsSuccess, string? Error)
{
    public static DomainResult Success() => new(true, null);

    public static DomainResult Failure(string error) => new(false, error);
}
