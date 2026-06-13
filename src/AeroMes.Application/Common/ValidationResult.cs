namespace AeroMes.Application.Common;

public record ValidationResult<T>
{
    public bool IsSuccess { get; private init; }
    public T? Value { get; private init; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; private init; }
    public string? ErrorMessage { get; private init; }
    public bool IsNotFound { get; private init; }

    private ValidationResult() { }

    public static ValidationResult<T> Ok(T value) =>
        new() { IsSuccess = true, Value = value };

    public static ValidationResult<T> Invalid(IReadOnlyDictionary<string, string[]> errors) =>
        new() { IsSuccess = false, Errors = errors };

    public static ValidationResult<T> Failure(string error) =>
        new() { IsSuccess = false, ErrorMessage = error };

    public static ValidationResult<T> NotFound(string error) =>
        new() { IsSuccess = false, ErrorMessage = error, IsNotFound = true };
}
