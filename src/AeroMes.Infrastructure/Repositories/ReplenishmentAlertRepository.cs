using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ReplenishmentAlertRepository(AppDbContext db) : IReplenishmentAlertRepository
{
    public async Task<IReadOnlyList<ReplenishmentAlert>> GetAllAsync(
        ReplenishmentAlertStatus? status, CancellationToken ct)
    {
        var q = db.ReplenishmentAlerts.AsNoTracking().AsQueryable();
        if (status.HasValue)
            q = q.Where(a => a.Status == status.Value);
        return await q.OrderByDescending(a => a.TriggeredAt).ToListAsync(ct);
    }

    public async Task<ReplenishmentAlert?> GetByIdAsync(long id, CancellationToken ct) =>
        await db.ReplenishmentAlerts
            .FirstOrDefaultAsync(a => a.AlertId == id, ct);

    public async Task<ReplenishmentAlert?> GetOpenByPolicyAsync(int policyId, CancellationToken ct) =>
        await db.ReplenishmentAlerts
            .FirstOrDefaultAsync(a =>
                a.PolicyId == policyId &&
                a.Status != ReplenishmentAlertStatus.Resolved, ct);

    public async Task AddAsync(ReplenishmentAlert alert, CancellationToken ct) =>
        await db.ReplenishmentAlerts.AddAsync(alert, ct);
}
