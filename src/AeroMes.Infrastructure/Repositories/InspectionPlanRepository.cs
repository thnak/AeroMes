using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class InspectionPlanRepository(AppDbContext db) : IInspectionPlanRepository
{
    public Task<InspectionPlan?> GetByIdAsync(int planId, CancellationToken ct) =>
        db.InspectionPlans.FirstOrDefaultAsync(x => x.PlanId == planId, ct);

    public Task<InspectionPlan?> GetByIdWithCharacteristicsAsync(int planId, CancellationToken ct) =>
        db.InspectionPlans
            .Include(p => p.Characteristics.OrderBy(c => c.Sequence))
            .FirstOrDefaultAsync(x => x.PlanId == planId, ct);

    public Task<InspectionPlan?> GetByCodeAsync(string code, CancellationToken ct) =>
        db.InspectionPlans.FirstOrDefaultAsync(x => x.Code == code.ToUpperInvariant(), ct);

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct) =>
        db.InspectionPlans.AnyAsync(x => x.Code == code.ToUpperInvariant(), ct);

    public Task<bool> HasLinkedInspectionOrdersAsync(int planId, CancellationToken ct)
    {
        // InspectionOrder table does not exist yet; return false for now.
        return Task.FromResult(false);
    }

    public async Task<IReadOnlyList<InspectionPlan>> GetListAsync(
        int? routingStepId, string? productCode, bool? isActive, CancellationToken ct)
    {
        var query = db.InspectionPlans
            .Include(p => p.Characteristics)
            .AsNoTracking()
            .AsQueryable();

        if (routingStepId.HasValue)
            query = query.Where(x => x.RoutingStepId == routingStepId.Value);

        if (!string.IsNullOrWhiteSpace(productCode))
            query = query.Where(x => x.ProductCode == productCode.ToUpperInvariant());

        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        return await query.OrderBy(x => x.Code).ToListAsync(ct);
    }

    public void Add(InspectionPlan plan) => db.InspectionPlans.Add(plan);

    public void Remove(InspectionPlan plan) => db.InspectionPlans.Remove(plan);
}
