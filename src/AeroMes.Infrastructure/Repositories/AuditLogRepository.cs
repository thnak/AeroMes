using AeroMes.Application.Interfaces;
using AeroMes.Domain.Auth;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class AuditLogRepository(AppDbContext db) : IAuditLogRepository
{
    public async Task<(IReadOnlyList<SecurityAuditLog> Items, int Total)> QueryAsync(
        string? actorId, string? eventType, string? targetType,
        DateTime? from, DateTime? to, int page, int pageSize,
        CancellationToken ct = default)
    {
        var query = db.SecurityAuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(actorId))
            query = query.Where(x => x.ActorId == actorId);
        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(x => x.EventType == eventType);
        if (!string.IsNullOrWhiteSpace(targetType))
            query = query.Where(x => x.TargetType == targetType);
        if (from.HasValue) query = query.Where(x => x.OccurredAt >= from.Value);
        if (to.HasValue) query = query.Where(x => x.OccurredAt <= to.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<SecurityAuditLog>> GetByUserAsync(
        string userId, int page, int pageSize, CancellationToken ct = default)
        => await db.SecurityAuditLogs
            .AsNoTracking()
            .Where(x => x.ActorId == userId || x.TargetId == userId)
            .OrderByDescending(x => x.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SecurityAuditLog>> GetForExportAsync(
        DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var query = db.SecurityAuditLogs.AsNoTracking();
        if (from.HasValue) query = query.Where(x => x.OccurredAt >= from.Value);
        if (to.HasValue) query = query.Where(x => x.OccurredAt <= to.Value);
        return await query
            .OrderByDescending(x => x.OccurredAt)
            .Take(10_000)
            .ToListAsync(ct);
    }
}
