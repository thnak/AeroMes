using AeroMes.Domain.Integration;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ProductionScheduleRepository(AppDbContext db) : IProductionScheduleRepository
{
    public async Task<int> AddAsync(ProductionSchedule schedule, CancellationToken ct)
    {
        db.ProductionSchedules.Add(schedule);
        await db.SaveChangesAsync(ct);
        return schedule.ScheduleId;
    }

    public Task<ProductionSchedule?> GetByIdAsync(int id, CancellationToken ct)
        => db.ProductionSchedules
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.ScheduleId == id, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public async Task DeleteAsync(ProductionSchedule schedule, CancellationToken ct)
    {
        db.ProductionSchedules.Remove(schedule);
        await db.SaveChangesAsync(ct);
    }

    public async Task<(IReadOnlyList<ScheduleListDto> Items, int Total)> GetListAsync(
        string? status, DateTime? from, DateTime? to, int page, int pageSize, CancellationToken ct)
    {
        var q = db.ProductionSchedules.AsNoTracking();
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ScheduleStatus>(status, true, out var statusEnum))
            q = q.Where(s => s.Status == statusEnum);
        if (from.HasValue) q = q.Where(s => s.PeriodEnd >= from.Value);
        if (to.HasValue) q = q.Where(s => s.PeriodStart <= to.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(s => new ScheduleListDto(
                s.ScheduleId, s.ScheduleName, s.FacilityCode,
                s.PeriodStart, s.PeriodEnd, s.Status.ToString(),
                s.Lines.Count, s.CreatedAt))
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<ScheduleDetailDto?> GetDetailAsync(int id, CancellationToken ct)
    {
        var schedule = await db.ProductionSchedules.AsNoTracking()
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.ScheduleId == id, ct);
        if (schedule is null) return null;

        var poIds = schedule.Lines.Select(l => l.POID).Distinct().ToList();
        var wcIds = schedule.Lines.Select(l => l.WorkCenterID).Distinct().ToList();

        var orders = await db.ProductionOrders.AsNoTracking()
            .Where(po => poIds.Contains(po.POID))
            .Join(db.Products, po => po.ProductCode, p => p.ProductCode,
                (po, p) => new { po.POID, po.POCode, po.ProductCode, ProductName = p.ProductName })
            .ToDictionaryAsync(x => x.POID, ct);

        var wcs = await db.WorkCenters.AsNoTracking()
            .Where(wc => wcIds.Contains(wc.WorkCenterID))
            .Select(wc => new { wc.WorkCenterID, wc.WorkCenterCode, wc.WorkCenterName })
            .ToDictionaryAsync(x => x.WorkCenterID, ct);

        var lines = schedule.Lines.Select(l =>
        {
            orders.TryGetValue(l.POID, out var po);
            wcs.TryGetValue(l.WorkCenterID, out var wc);
            return new ScheduleLineDto(
                l.LineId, l.POID, po?.POCode ?? string.Empty, po?.ProductCode ?? string.Empty,
                po?.ProductName, l.WorkCenterID,
                wc?.WorkCenterCode ?? string.Empty, wc?.WorkCenterName ?? string.Empty,
                l.PlannedStart, l.PlannedEnd, l.SequenceNo, l.Notes);
        }).ToList();

        return new ScheduleDetailDto(
            schedule.ScheduleId, schedule.ScheduleName, schedule.FacilityCode,
            schedule.PeriodStart, schedule.PeriodEnd,
            schedule.Status.ToString(), schedule.CreatedAt, lines);
    }

    public async Task<IReadOnlyList<PendingOrderDto>> GetPendingOrdersAsync(
        DateTime periodStart, DateTime periodEnd, int? excludeScheduleId, CancellationToken ct)
    {
        // Orders not already scheduled in this schedule
        var scheduledPoIds = excludeScheduleId.HasValue
            ? await db.ProductionScheduleLines.AsNoTracking()
                .Where(l => l.ScheduleId == excludeScheduleId.Value)
                .Select(l => l.POID)
                .ToListAsync(ct)
            : [];

        return await db.ProductionOrders.AsNoTracking()
            .Where(po =>
                po.Status != ProductionOrderStatus.Completed &&
                po.Status != ProductionOrderStatus.Cancelled &&
                (po.PlannedStartDate == null || po.PlannedStartDate <= periodEnd) &&
                (po.PlannedEndDate == null || po.PlannedEndDate >= periodStart) &&
                !scheduledPoIds.Contains(po.POID))
            .Join(db.Products, po => po.ProductCode, p => p.ProductCode,
                (po, p) => new PendingOrderDto(
                    po.POID, po.POCode, po.ProductCode, p.ProductName,
                    po.TargetQuantity, po.PlannedStartDate, po.PlannedEndDate,
                    po.ProductionDeadline, po.Priority))
            .OrderBy(o => o.Priority)
            .ThenBy(o => o.ProductionDeadline ?? o.PlannedEnd)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<int, List<(DateTime Start, DateTime End)>>> GetScheduledSlotsByWorkCenterAsync(
        DateTime periodStart, DateTime periodEnd, int scheduleId, CancellationToken ct)
    {
        var lines = await db.ProductionScheduleLines.AsNoTracking()
            .Where(l => l.ScheduleId == scheduleId
                && l.PlannedEnd >= periodStart
                && l.PlannedStart <= periodEnd)
            .Select(l => new { l.WorkCenterID, l.PlannedStart, l.PlannedEnd })
            .ToListAsync(ct);

        return lines
            .GroupBy(l => l.WorkCenterID)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(l => l.PlannedStart)
                    .Select(l => (l.PlannedStart, l.PlannedEnd))
                    .ToList());
    }

    public async Task<CapacityInfo?> GetPrimaryCapacityAsync(string productCode, CancellationToken ct)
    {
        var routing = await db.Routings.AsNoTracking()
            .Where(r => r.ProductCode == productCode && r.IsActive && r.IsDefault)
            .FirstOrDefaultAsync(ct);
        if (routing is null) return null;

        var step = await db.RoutingSteps.AsNoTracking()
            .Where(rs => rs.RoutingID == routing.RoutingID)
            .OrderBy(rs => rs.StepNumber)
            .FirstOrDefaultAsync(ct);
        if (step is null) return null;

        return new CapacityInfo(step.DefaultWorkCenterID, step.StandardCycleTime);
    }
}
