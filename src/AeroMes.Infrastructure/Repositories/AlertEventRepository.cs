using AeroMes.Domain.Alert;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class AlertEventRepository(AppDbContext db) : IAlertEventRepository
{
    public async Task AddAsync(AlertEvent alertEvent, CancellationToken ct)
    {
        db.AlertEvents.Add(alertEvent);
        await db.SaveChangesAsync(ct);
    }

    public Task<AlertEvent?> GetByIdAsync(long id, CancellationToken ct)
        => db.AlertEvents.FirstOrDefaultAsync(a => a.AlertEventId == id, ct);

    public async Task<IReadOnlyList<AlertEventDto>> GetListAsync(
        bool? isActive, int? thresholdId, int page, int pageSize, CancellationToken ct)
    {
        var q = db.AlertEvents.AsNoTracking()
            .Include(a => a.Threshold)
            .AsQueryable();

        if (isActive.HasValue) q = q.Where(a => a.IsActive == isActive.Value);
        if (thresholdId.HasValue) q = q.Where(a => a.ThresholdId == thresholdId.Value);

        return await q.OrderByDescending(a => a.TriggeredAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new AlertEventDto(
                a.AlertEventId, a.ThresholdId,
                a.Threshold!.MetricKey,
                a.Threshold.Scope,
                a.ScopeId,
                a.Level.ToString(),
                a.MetricValue, a.TriggeredAt, a.AcknowledgedAt, a.AcknowledgedBy,
                a.IsActive, a.Message))
            .ToListAsync(ct);
    }

    public async Task<DateTimeOffset?> GetLastTriggeredAtAsync(int thresholdId, string? scopeId, CancellationToken ct)
    {
        var q = db.AlertEvents.AsNoTracking()
            .Where(a => a.ThresholdId == thresholdId);

        if (scopeId is null)
            q = q.Where(a => a.ScopeId == null);
        else
            q = q.Where(a => a.ScopeId == scopeId);

        return await q.MaxAsync(a => (DateTimeOffset?)a.TriggeredAt, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
