using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class AlertThresholdRepository(AppDbContext db) : IAlertThresholdRepository
{
    public Task<AlertThreshold?> GetByIdAsync(int id, CancellationToken ct) =>
        db.AlertThresholds.FirstOrDefaultAsync(x => x.ThresholdId == id, ct);

    public async Task<IReadOnlyList<AlertThreshold>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.AlertThresholds.AsQueryable();
        if (activeOnly) q = q.Where(x => x.IsActive);
        return await q.OrderBy(x => x.Scope).ThenBy(x => x.MetricKey).ToListAsync(ct);
    }

    public Task AddAsync(AlertThreshold entity, CancellationToken ct)
    {
        db.AlertThresholds.Add(entity);
        return Task.CompletedTask;
    }
}
