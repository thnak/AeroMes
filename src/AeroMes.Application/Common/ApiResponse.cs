namespace AeroMes.Application.Common;

public record ApiResponse<T>(bool Success, string Message, T? Data = default);

public record ApiResponse(bool Success, string Message);
