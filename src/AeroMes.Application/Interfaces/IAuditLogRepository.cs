using AeroMes.Domain.Auth;

namespace AeroMes.Application.Interfaces;

public interface IAuditLogRepository
{
    Task<(IReadOnlyList<SecurityAuditLog> Items, int Total)> QueryAsync(
        string? actorId, string? eventType, string? targetType,
        DateTime? from, DateTime? to, int page, int pageSize,
        CancellationToken ct = default);

    Task<IReadOnlyList<SecurityAuditLog>> GetByUserAsync(
        string userId, int page, int pageSize, CancellationToken ct = default);

    Task<IReadOnlyList<SecurityAuditLog>> GetForExportAsync(
        DateTime? from, DateTime? to, CancellationToken ct = default);
}
