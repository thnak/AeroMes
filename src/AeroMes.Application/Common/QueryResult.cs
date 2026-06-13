namespace AeroMes.Application.Common;

public sealed class QueryResult<T>
{
    public bool IsFound { get; private init; }
    public T? Value { get; private init; }
    public string? ErrorMessage { get; private init; }

    private QueryResult() { }

    public static QueryResult<T> Found(T value) =>
        new() { IsFound = true, Value = value };

    public static QueryResult<T> NotFound(string message) =>
        new() { IsFound = false, ErrorMessage = message };
}
