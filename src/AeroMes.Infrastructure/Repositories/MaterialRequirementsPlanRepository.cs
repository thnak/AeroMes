using AeroMes.Domain.Master;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class MaterialRequirementsPlanRepository(AppDbContext db) : IMaterialRequirementsPlanRepository
{
    public async Task<int> AddAsync(MaterialRequirementsPlan plan, CancellationToken ct)
    {
        db.MaterialRequirementsPlans.Add(plan);
        await db.SaveChangesAsync(ct);
        return plan.MrpID;
    }

    public Task<MaterialRequirementsPlan?> GetByIdAsync(int mrpId, CancellationToken ct)
        => db.MaterialRequirementsPlans
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.MrpID == mrpId, ct);

    public Task<bool> PlanNumberExistsAsync(string planNumber, CancellationToken ct)
        => db.MaterialRequirementsPlans.AnyAsync(p => p.PlanNumber == planNumber, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public async Task DeleteAsync(MaterialRequirementsPlan plan, CancellationToken ct)
    {
        db.MaterialRequirementsPlans.Remove(plan);
        await db.SaveChangesAsync(ct);
    }

    public async Task<(IReadOnlyList<MrpListDto> Items, int Total)> GetListAsync(
        string? keyword, int? masterPlanId, string? status, int page, int pageSize, CancellationToken ct)
    {
        var q = db.MaterialRequirementsPlans.AsNoTracking();
        if (!string.IsNullOrEmpty(keyword))
            q = q.Where(p => p.PlanNumber.Contains(keyword) || p.PlanName.Contains(keyword));
        if (masterPlanId.HasValue)
            q = q.Where(p => p.MasterPlanId == masterPlanId.Value);
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<MrpStatus>(status, true, out var statusEnum))
            q = q.Where(p => p.Status == statusEnum);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new MrpListDto(
                p.MrpID, p.PlanNumber, p.PlanName,
                p.MasterPlanId, p.OrganizationalUnit,
                p.PeriodStart, p.PeriodEnd,
                p.Status.ToString(),
                p.Lines.Count, p.CreatedAt))
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<MrpDetailDto?> GetDetailAsync(int mrpId, CancellationToken ct)
    {
        var plan = await db.MaterialRequirementsPlans.AsNoTracking()
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.MrpID == mrpId, ct);

        if (plan is null) return null;

        var lines = plan.Lines.Select(l => new MrpLineDto(
            l.MrpLineID, l.FinishedGoodCode, l.FinishedGoodQty,
            l.MaterialCode, l.MaterialName, l.UnitOfMeasure,
            l.FixedWaste, l.WasteRatio, l.CalculatedMaterialQty,
            l.OpeningInventory, l.ConcurrentPurchaseRequestQty,
            l.PlannedOrderQty, l.ForecastedClosingBalance, l.HasShortfall))
            .ToList();

        return new MrpDetailDto(
            plan.MrpID, plan.PlanNumber, plan.PlanName,
            plan.MasterPlanId, plan.OrganizationalUnit,
            plan.PeriodStart, plan.PeriodEnd,
            plan.Status.ToString(), plan.Notes, plan.CreatedAt, lines);
    }

    public async Task<IReadOnlyList<BomExplosionItem>> ExplodeBomAsync(
        int masterPlanId, DateOnly date, CancellationToken ct)
    {
        var planLines = await db.ProductionPlanOrderLines.AsNoTracking()
            .Where(l => l.PlanId == masterPlanId)
            .Select(l => new { l.ProductCode, l.PlannedQty })
            .ToListAsync(ct);

        var productCodes = planLines.Select(l => l.ProductCode).Distinct().ToList();

        var bomLines = await db.BomHeaders.AsNoTracking()
            .Where(h => productCodes.Contains(h.ProductCode)
                && h.Status == BomStatus.Active
                && (h.EffectiveFrom == null || h.EffectiveFrom <= date)
                && (h.EffectiveTo == null || h.EffectiveTo >= date))
            .Join(db.BomLines, h => h.BomHeaderId, l => l.BomHeaderId,
                (h, l) => new
                {
                    h.ProductCode, h.BaseQuantity,
                    l.ComponentCode, l.RequiredQty, l.UoMCode, l.ScrapFactor
                })
            .Join(db.Products, b => b.ComponentCode, p => p.ProductCode,
                (b, p) => new { b, MaterialName = p.ProductName })
            .ToListAsync(ct);

        var result = new List<BomExplosionItem>();
        foreach (var plan in planLines)
        {
            foreach (var b in bomLines.Where(b => b.b.ProductCode == plan.ProductCode))
            {
                result.Add(new BomExplosionItem(
                    plan.ProductCode, plan.PlannedQty,
                    b.b.ComponentCode, b.MaterialName, b.b.UoMCode,
                    b.b.BaseQuantity > 0 ? b.b.RequiredQty / b.b.BaseQuantity : b.b.RequiredQty,
                    b.b.ScrapFactor));
            }
        }
        return result;
    }
}
