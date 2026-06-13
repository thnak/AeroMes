using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class CycleCountPlanRepository(AppDbContext db) : ICycleCountPlanRepository
{
    public async Task<IReadOnlyList<CycleCountPlan>> GetAllAsync(
        CycleCountPlanStatus? status, CancellationToken ct)
    {
        var q = db.CycleCountPlans.AsNoTracking().AsQueryable();
        if (status.HasValue)
            q = q.Where(p => p.Status == status.Value);
        return await q.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);
    }

    public async Task<CycleCountPlan?> GetByIdAsync(int id, CancellationToken ct) =>
        await db.CycleCountPlans.AsNoTracking()
            .FirstOrDefaultAsync(p => p.PlanId == id, ct);

    public async Task<CycleCountPlan?> GetByIdWithLinesAsync(int id, CancellationToken ct) =>
        await db.CycleCountPlans
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.PlanId == id, ct);

    public async Task<bool> PlanCodeExistsAsync(string planCode, CancellationToken ct) =>
        await db.CycleCountPlans.AsNoTracking()
            .AnyAsync(p => p.PlanCode == planCode, ct);

    public async Task AddAsync(CycleCountPlan plan, CancellationToken ct) =>
        await db.CycleCountPlans.AddAsync(plan, ct);
}
