namespace AeroMes.Application.Common;

public record ApiResponse<T>(bool Success, string Message, T? Data = default);
public record ApiResponse(bool Success, string Message);
public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
public record MessageResult(string Message);
public record IdentityErrorResult(IEnumerable<string> Errors);
