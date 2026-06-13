using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MasterProductionPlanRepository(AppDbContext db) : IMasterProductionPlanRepository
{
    public Task<MasterProductionPlan?> GetByIdAsync(int id, CancellationToken ct) =>
        db.MasterProductionPlans.FirstOrDefaultAsync(x => x.MasterPlanId == id, ct);

    public Task<MasterProductionPlan?> GetByIdWithLinesAsync(int id, CancellationToken ct) =>
        db.MasterProductionPlans
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.MasterPlanId == id, ct);

    public async Task<IReadOnlyList<MasterPlanSummaryDto>> GetListAsync(
        string? orgUnit, string? status, DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        var q = db.MasterProductionPlans.AsNoTracking().AsQueryable();

        if (orgUnit is not null)
            q = q.Where(x => x.OrganizationalUnit != null && x.OrganizationalUnit.Contains(orgUnit));
        if (status is not null && Enum.TryParse<MpsStatus>(status, ignoreCase: true, out var s))
            q = q.Where(x => x.Status == s);
        if (from is not null)
            q = q.Where(x => x.PeriodEnd >= from);
        if (to is not null)
            q = q.Where(x => x.PeriodStart <= to);

        return await q
            .OrderByDescending(x => x.MasterPlanId)
            .Select(x => new MasterPlanSummaryDto(
                x.MasterPlanId, x.PlanNumber, x.PlanName,
                x.OrganizationalUnit,
                x.Granularity.ToString(),
                x.PeriodStart, x.PeriodEnd,
                x.DataSource.ToString(),
                x.Status.ToString(),
                x.Lines.Count,
                x.CreatedBy,
                x.CreatedAt))
            .ToListAsync(ct);
    }

    public Task<bool> ExistsByPlanNumberAsync(string planNumber, CancellationToken ct) =>
        db.MasterProductionPlans.AnyAsync(x => x.PlanNumber == planNumber, ct);

    public async Task<string> NextPlanNumberAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var count = await db.MasterProductionPlans
            .CountAsync(x => x.CreatedAt.Year == year, ct);
        return $"MPS-{year}-{count + 1:D4}";
    }

    public Task AddAsync(MasterProductionPlan entity, CancellationToken ct)
    {
        db.MasterProductionPlans.Add(entity);
        return Task.CompletedTask;
    }

    public void Remove(MasterProductionPlan entity) =>
        db.MasterProductionPlans.Remove(entity);

    public async Task SaveChangesAsync(CancellationToken ct) =>
        await db.SaveChangesAsync(ct);
}
