using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class DetailedProductionPlanRepository(AppDbContext db) : IDetailedProductionPlanRepository
{
    public Task<DetailedProductionPlan?> GetByIdAsync(int id, CancellationToken ct) =>
        db.DetailedProductionPlans.FirstOrDefaultAsync(x => x.DetailPlanId == id, ct);

    public Task<DetailedProductionPlan?> GetByIdWithLinesAsync(int id, CancellationToken ct) =>
        db.DetailedProductionPlans
            .Include(x => x.ProductLines)
                .ThenInclude(l => l.Slots)
            .FirstOrDefaultAsync(x => x.DetailPlanId == id, ct);

    public async Task<IReadOnlyList<DetailedPlanSummaryDto>> GetListAsync(
        int? masterPlanId, string? orgUnit, string? status, CancellationToken ct)
    {
        var q = db.DetailedProductionPlans.AsNoTracking().AsQueryable();

        if (masterPlanId is not null)
            q = q.Where(x => x.MasterPlanId == masterPlanId);
        if (orgUnit is not null)
            q = q.Where(x => x.OrganizationalUnit != null && x.OrganizationalUnit.Contains(orgUnit));
        if (status is not null && Enum.TryParse<DppStatus>(status, ignoreCase: true, out var s))
            q = q.Where(x => x.Status == s);

        return await q
            .OrderByDescending(x => x.DetailPlanId)
            .Select(x => new DetailedPlanSummaryDto(
                x.DetailPlanId, x.PlanNumber, x.PlanName,
                x.MasterPlanId,
                db.MasterProductionPlans
                    .Where(m => m.MasterPlanId == x.MasterPlanId)
                    .Select(m => m.PlanNumber)
                    .FirstOrDefault(),
                x.OrganizationalUnit,
                x.PeriodStart, x.PeriodEnd,
                x.Granularity.ToString(),
                x.Status.ToString(),
                x.HasProductionOrders,
                x.ProductLines.Count,
                x.CreatedBy,
                x.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<string> NextPlanNumberAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.DetailedProductionPlans
            .CountAsync(x => x.CreatedAt.Year == year, ct);
        return $"DPP-{year}-{count + 1:D4}";
    }

    public Task AddAsync(DetailedProductionPlan entity, CancellationToken ct)
    {
        db.DetailedProductionPlans.Add(entity);
        return Task.CompletedTask;
    }

    public void Remove(DetailedProductionPlan entity) =>
        db.DetailedProductionPlans.Remove(entity);

    public async Task SaveChangesAsync(CancellationToken ct) =>
        await db.SaveChangesAsync(ct);
}
