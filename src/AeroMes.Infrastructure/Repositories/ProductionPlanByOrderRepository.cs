using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ProductionPlanByOrderRepository(AppDbContext db) : IProductionPlanByOrderRepository
{
    public Task AddAsync(ProductionPlanByOrder plan, CancellationToken ct = default)
    {
        db.ProductionPlansByOrder.Add(plan);
        return Task.CompletedTask;
    }

    public Task<ProductionPlanByOrder?> GetByIdAsync(int planId, CancellationToken ct = default)
        => db.ProductionPlansByOrder.Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.PlanId == planId, ct);

    public Task<bool> CodeExistsAsync(string planCode, CancellationToken ct = default)
        => db.ProductionPlansByOrder.AnyAsync(p => p.PlanCode == planCode, ct);

    public async Task<(IReadOnlyList<ProductionPlanDto> Items, int Total)> GetListAsync(
        int? poId, ProductionPlanStatus? status,
        DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.ProductionPlansByOrder.AsNoTracking()
            .Include(p => p.Lines).AsQueryable();

        if (poId.HasValue) q = q.Where(p => p.PoId == poId.Value);
        if (status.HasValue) q = q.Where(p => p.Status == status.Value);
        if (fromDate.HasValue) q = q.Where(p => p.CreatedAt >= fromDate.Value);
        if (toDate.HasValue) q = q.Where(p => p.CreatedAt <= toDate.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductionPlanDto(
                p.PlanId, p.PlanCode, p.PoId,
                p.AllocationMethod.ToString(),
                p.Status.ToString(),
                p.Notes, p.CreatedAt, p.ConfirmedAt,
                p.Lines.Select(l => new ProductionPlanLineDto(
                    l.PlanLineId, l.ProductCode, l.PlannedQty,
                    l.TeamCode, l.PlannedStartDate, l.PlannedEndDate,
                    l.ActualQty, l.IsLate))
                .ToList()))
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<GanttTeamDto>> GetGanttDataAsync(
        DateTime fromDate, DateTime toDate, string? teamCode, CancellationToken ct = default)
    {
        var q = db.ProductionPlansByOrder.AsNoTracking()
            .Include(p => p.Lines)
            .Where(p => p.Status != ProductionPlanStatus.Cancelled
                && p.Status != ProductionPlanStatus.Draft
                && p.Lines.Any(l =>
                    l.PlannedStartDate.HasValue && l.PlannedEndDate.HasValue
                    && l.PlannedEndDate.Value >= fromDate
                    && l.PlannedStartDate.Value <= toDate));

        if (!string.IsNullOrWhiteSpace(teamCode))
            q = q.Where(p => p.Lines.Any(l => l.TeamCode == teamCode.ToUpperInvariant()));

        var plans = await q.ToListAsync(ct);

        var byTeam = plans
            .SelectMany(p => p.Lines
                .Where(l => l.TeamCode != null
                    && l.PlannedStartDate.HasValue && l.PlannedEndDate.HasValue
                    && l.PlannedEndDate!.Value >= fromDate
                    && l.PlannedStartDate!.Value <= toDate)
                .Select(l => new
                {
                    l.TeamCode,
                    Interval = new GanttIntervalDto(
                        l.PlanLineId, p.PlanId, p.PlanCode, l.ProductCode,
                        p.Status.ToString(),
                        l.PlannedStartDate!.Value, l.PlannedEndDate!.Value,
                        l.PlannedQty, l.ActualQty, l.IsLate)
                }))
            .GroupBy(x => x.TeamCode!)
            .Select(g => new GanttTeamDto(g.Key, g.Key, g.Select(x => x.Interval).ToList()))
            .ToList();

        return byTeam;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
