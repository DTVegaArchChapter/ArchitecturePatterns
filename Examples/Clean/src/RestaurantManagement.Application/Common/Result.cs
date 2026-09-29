using FluentValidation.Results;

namespace RestaurantManagement.Application.Common;

public enum ResultType
{
    Success,
    NotFound,
    Conflict,
    Failure
}

public interface IOperationResult
{
    bool IsSuccess { get; }
    string? ErrorMessage { get; }
    ResultType ResultType { get; }
    IReadOnlyDictionary<string, object> ErrorDetails { get; }
}

public interface IResultFactory<TSelf> where TSelf : IResultFactory<TSelf>
{
    static abstract TSelf From(ValidationResult validationResult);
}

public sealed class Result<T> : IOperationResult, IResultFactory<Result<T>>
{
    private static readonly IReadOnlyDictionary<string, object> Empty = new Dictionary<string, object>();

    private IReadOnlyDictionary<string, object>? _errorDetails;
    public bool IsSuccess { get; private init; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private init; }
    public ResultType ResultType { get; private init; }

    public IReadOnlyDictionary<string, object> ErrorDetails => _errorDetails ?? Empty;

    private Result() { }

    public static Result<T> Success(T data)
    {
        return new Result<T>
        {
            IsSuccess = true,
            Data = data,
            ResultType = ResultType.Success
        };
    }

    public static Result<T> Failure(string errorMessage, ResultType resultType = ResultType.Failure, IReadOnlyDictionary<string, object>? errorDetails = null)
    {
        return new Result<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            ResultType = resultType,
            _errorDetails = errorDetails
        };
    }

    public static Result<T> NotFound(string errorMessage, IReadOnlyDictionary<string, object>? errorDetails = null)
    {
        return Failure(errorMessage, ResultType.NotFound, errorDetails);
    }

    public static Result<T> Conflict(string errorMessage, IReadOnlyDictionary<string, object>? errorDetails = null)
    {
        return Failure(errorMessage, ResultType.Conflict, errorDetails);
    }

    public static Result<T> From(ValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        var errors = validationResult.Errors;
        var message = $"Validation failed: {string.Join("; ", errors.Select(e => e.ErrorMessage))}";
        var details = errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => (object)g.Select(e => e.ErrorMessage).ToArray());

        return Failure(message, ResultType.Failure, details);
    }
}
