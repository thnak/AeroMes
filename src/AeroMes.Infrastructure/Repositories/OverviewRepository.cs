using AeroMes.Application.Interfaces;
using AeroMes.Domain.Integration;
using AeroMes.Domain.Production;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class OverviewRepository(AppDbContext db) : IOverviewRepository
{
    public async Task<IncompleteOrdersResult> GetIncompleteOrdersAsync(DateTime? from, DateTime? to, CancellationToken ct)
    {
        var woQ = db.WorkOrders.AsNoTracking();
        if (from.HasValue) woQ = woQ.Where(w => w.ActualStartDate == null || w.ActualStartDate >= from.Value);
        if (to.HasValue) woQ = woQ.Where(w => w.ActualStartDate == null || w.ActualStartDate <= to.Value);

        var totalWOs = await woQ.CountAsync(ct);
        var incompleteWOs = await woQ.CountAsync(
            w => w.Status != WorkOrderStatus.Completed && w.Status != WorkOrderStatus.Cancelled, ct);
        var completionPct = totalWOs > 0
            ? Math.Round((double)(totalWOs - incompleteWOs) / totalWOs * 100, 1)
            : 100.0;

        var poQ = db.ProductionOrders.AsNoTracking();
        if (from.HasValue) poQ = poQ.Where(p => p.PlannedStartDate == null || p.PlannedStartDate >= from.Value);
        if (to.HasValue) poQ = poQ.Where(p => p.PlannedStartDate == null || p.PlannedStartDate <= to.Value);

        var totalPOs = await poQ.CountAsync(ct);
        var incompletePOs = await poQ.CountAsync(
            p => p.Status != ProductionOrderStatus.Completed && p.Status != ProductionOrderStatus.Cancelled, ct);

        return new IncompleteOrdersResult(totalWOs, incompleteWOs, completionPct, totalPOs, incompletePOs);
    }

    public async Task<IReadOnlyList<RemainingVolumeItem>> GetRemainingVolumeAsync(DateTime? from, DateTime? to, CancellationToken ct)
    {
        var q = db.WorkOrders.AsNoTracking()
            .Where(w => w.Status != WorkOrderStatus.Completed && w.Status != WorkOrderStatus.Cancelled);
        if (from.HasValue) q = q.Where(w => w.ActualStartDate == null || w.ActualStartDate >= from.Value);
        if (to.HasValue) q = q.Where(w => w.ActualStartDate == null || w.ActualStartDate <= to.Value);

        return await q
            .Join(db.ProductionOrders, w => w.POID, po => po.POID, (w, po) => new { w, po.ProductCode })
            .Join(db.Products, x => x.ProductCode, p => p.ProductCode,
                (x, p) => new { x.w, x.ProductCode, ProductName = p.ProductName })
            .GroupBy(x => new { x.ProductCode, x.ProductName })
            .Select(g => new RemainingVolumeItem(
                g.Key.ProductCode,
                g.Key.ProductName,
                g.Sum(x => x.w.TargetQuantity.Value),
                g.Sum(x => x.w.ActualQtyOK.Value),
                g.Sum(x => x.w.TargetQuantity.Value) - g.Sum(x => x.w.ActualQtyOK.Value)))
            .OrderByDescending(x => x.RemainingQty)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OutputOverTimeItem>> GetOutputOverTimeAsync(
        DateTime from, DateTime to, string granularity, CancellationToken ct)
    {
        var daily = await db.ProductionLogs.AsNoTracking()
            .Where(pl => pl.Timestamp >= from && pl.Timestamp <= to)
            .GroupBy(pl => pl.Timestamp.Date)
            .Select(g => new { Date = g.Key, TotalQtyOK = g.Sum(x => x.QtyOK) })
            .OrderBy(x => x.Date)
            .ToListAsync(ct);

        return granularity.ToLowerInvariant() switch
        {
            "month" => daily
                .GroupBy(x => new { x.Date.Year, x.Date.Month })
                .Select(g => new OutputOverTimeItem($"{g.Key.Year}-{g.Key.Month:D2}", g.Sum(x => x.TotalQtyOK)))
                .OrderBy(x => x.Period)
                .ToList(),
            "week" => daily
                .GroupBy(x => $"{x.Date.Year}-W{System.Globalization.ISOWeek.GetWeekOfYear(x.Date):D2}")
                .Select(g => new OutputOverTimeItem(g.Key, g.Sum(x => x.TotalQtyOK)))
                .OrderBy(x => x.Period)
                .ToList(),
            _ => daily
                .Select(x => new OutputOverTimeItem(x.Date.ToString("yyyy-MM-dd"), x.TotalQtyOK))
                .ToList()
        };
    }

    public async Task<IReadOnlyList<OrdersByStatusItem>> GetOrdersByStatusAsync(DateTime? from, DateTime? to, CancellationToken ct)
    {
        var q = db.WorkOrders.AsNoTracking();
        if (from.HasValue) q = q.Where(w => w.ActualStartDate == null || w.ActualStartDate >= from.Value);
        if (to.HasValue) q = q.Where(w => w.ActualStartDate == null || w.ActualStartDate <= to.Value);

        return await q
            .GroupBy(w => w.Status)
            .Select(g => new OrdersByStatusItem(g.Key.ToString(), g.Count()))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OutputByStageItem>> GetOutputByStageAsync(DateTime? from, DateTime? to, CancellationToken ct)
    {
        var q = db.WorkOrders.AsNoTracking();
        if (from.HasValue) q = q.Where(w => w.ActualStartDate >= from.Value);
        if (to.HasValue) q = q.Where(w => w.ActualStartDate <= to.Value);

        return await q
            .Join(db.RoutingSteps, w => w.RoutingStepID, rs => rs.RoutingStepID,
                (w, rs) => new { w, rs.OperationCode })
            .Join(db.Operations, x => x.OperationCode, op => op.OperationCode,
                (x, op) => new { x.w, x.OperationCode, op.OperationName })
            .GroupBy(x => new { x.OperationCode, x.OperationName })
            .Select(g => new OutputByStageItem(
                g.Key.OperationCode, g.Key.OperationName, g.Sum(x => x.w.ActualQtyOK.Value)))
            .OrderByDescending(x => x.TotalQtyOK)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OutputByDepartmentItem>> GetOutputByDepartmentAsync(DateTime? from, DateTime? to, CancellationToken ct)
    {
        var q = db.WorkOrders.AsNoTracking();
        if (from.HasValue) q = q.Where(w => w.ActualStartDate >= from.Value);
        if (to.HasValue) q = q.Where(w => w.ActualStartDate <= to.Value);

        return await q
            .Join(db.WorkCenters, w => w.WorkCenterID, wc => wc.WorkCenterID, (w, wc) => new { w, wc })
            .GroupBy(x => new { x.wc.WorkCenterCode, x.wc.WorkCenterName })
            .Select(g => new OutputByDepartmentItem(
                g.Key.WorkCenterCode, g.Key.WorkCenterName, g.Sum(x => x.w.ActualQtyOK.Value)))
            .OrderByDescending(x => x.TotalQtyOK)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TopProductByVolumeItem>> GetTopProductsByVolumeAsync(
        DateTime? from, DateTime? to, int topN, CancellationToken ct)
    {
        var q = db.WorkOrders.AsNoTracking();
        if (from.HasValue) q = q.Where(w => w.ActualStartDate >= from.Value);
        if (to.HasValue) q = q.Where(w => w.ActualStartDate <= to.Value);

        return await q
            .Join(db.ProductionOrders, w => w.POID, po => po.POID, (w, po) => new { w, po.ProductCode })
            .Join(db.Products, x => x.ProductCode, p => p.ProductCode,
                (x, p) => new { x.w, x.ProductCode, ProductName = p.ProductName })
            .GroupBy(x => new { x.ProductCode, x.ProductName })
            .Select(g => new TopProductByVolumeItem(
                g.Key.ProductCode, g.Key.ProductName, g.Sum(x => x.w.ActualQtyOK.Value)))
            .OrderByDescending(x => x.TotalQtyOK)
            .Take(topN)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TopProductByErrorRateItem>> GetTopProductsByErrorRateAsync(
        DateTime? from, DateTime? to, int topN, CancellationToken ct)
    {
        // DefectEntry.WorkOrderId is long; WorkOrder.WOID is int — join in two steps to avoid type mismatch
        DateTimeOffset? fromDto = from.HasValue ? new DateTimeOffset(from.Value, TimeSpan.Zero) : null;
        DateTimeOffset? toDto = to.HasValue ? new DateTimeOffset(to.Value, TimeSpan.Zero) : null;

        var defects = await db.DefectEntries.AsNoTracking()
            .Where(de => (!fromDto.HasValue || de.CreatedAt >= fromDto.Value)
                      && (!toDto.HasValue || de.CreatedAt <= toDto.Value))
            .Select(de => new { de.WorkOrderId, de.Quantity })
            .ToListAsync(ct);

        if (defects.Count == 0) return [];

        var woIdList = defects.Select(d => (int)d.WorkOrderId).Distinct().ToList();

        var woInfo = await db.WorkOrders.AsNoTracking()
            .Where(wo => woIdList.Contains(wo.WOID))
            .Join(db.ProductionOrders, wo => wo.POID, po => po.POID,
                (wo, po) => new { wo.WOID, po.ProductCode, QtyOK = wo.ActualQtyOK.Value })
            .Join(db.Products, x => x.ProductCode, p => p.ProductCode,
                (x, p) => new { x.WOID, x.ProductCode, ProductName = p.ProductName, x.QtyOK })
            .ToListAsync(ct);

        var woMap = woInfo.ToDictionary(x => (long)x.WOID);

        return defects
            .Where(d => woMap.ContainsKey(d.WorkOrderId))
            .GroupBy(d => new
            {
                woMap[d.WorkOrderId].ProductCode,
                woMap[d.WorkOrderId].ProductName
            })
            .Select(g =>
            {
                var totalDefects = g.Sum(x => x.Quantity);
                var totalQtyOK = g.Select(d => d.WorkOrderId).Distinct()
                    .Sum(id => woMap.TryGetValue(id, out var info) ? info.QtyOK : 0);
                var total = (double)(totalQtyOK + (int)totalDefects);
                var errorRate = total > 0 ? Math.Round((double)totalDefects / total * 100, 2) : 0;
                return new TopProductByErrorRateItem(
                    g.Key.ProductCode, g.Key.ProductName, totalQtyOK, totalDefects, errorRate);
            })
            .OrderByDescending(x => x.ErrorRate)
            .Take(topN)
            .ToList();
    }

    public async Task<IReadOnlyList<ErrorRateByCategoryItem>> GetErrorRateByCategoryAsync(
        DateTime? from, DateTime? to, CancellationToken ct)
    {
        DateTimeOffset? fromDto = from.HasValue ? new DateTimeOffset(from.Value, TimeSpan.Zero) : null;
        DateTimeOffset? toDto = to.HasValue ? new DateTimeOffset(to.Value, TimeSpan.Zero) : null;

        var deQ = db.DefectEntries.AsNoTracking()
            .Where(de => (!fromDto.HasValue || de.CreatedAt >= fromDto.Value)
                      && (!toDto.HasValue || de.CreatedAt <= toDto.Value));

        var rawGroups = await deQ
            .Join(db.DefectCodes, de => de.DefectCodeId, dc => dc.DefectCodeID,
                (de, dc) => new { Qty = de.Quantity, Cat = dc.DefectCategory })
            .GroupBy(x => x.Cat ?? "Uncategorized")
            .Select(g => new { Category = g.Key, TotalDefects = g.Sum(x => x.Qty) })
            .ToListAsync(ct);

        var grandTotal = rawGroups.Sum(x => x.TotalDefects);

        return rawGroups
            .Select(x => new ErrorRateByCategoryItem(
                x.Category,
                x.TotalDefects,
                grandTotal > 0 ? Math.Round((double)x.TotalDefects / (double)grandTotal * 100, 2) : 0))
            .OrderByDescending(x => x.TotalDefects)
            .ToList();
    }

    public async Task<IReadOnlyList<StoppageReasonItem>> GetStoppageReasonsAsync(
        DateTime? from, DateTime? to, CancellationToken ct)
    {
        var q = db.DowntimeLogs.AsNoTracking();
        if (from.HasValue) q = q.Where(d => d.StartTime >= from.Value);
        if (to.HasValue) q = q.Where(d => d.StartTime <= to.Value);

        var raw = await q
            .Select(d => new { d.ReasonCode, d.ReasonName, d.StartTime, d.EndTime })
            .ToListAsync(ct);

        return raw
            .GroupBy(d => new { d.ReasonCode, d.ReasonName })
            .Select(g => new StoppageReasonItem(
                g.Key.ReasonCode,
                g.Key.ReasonName,
                g.Count(),
                Math.Round(g.Sum(d => d.EndTime.HasValue
                    ? (d.EndTime.Value - d.StartTime).TotalMinutes : 0) / 60.0, 2)))
            .OrderByDescending(x => x.Count)
            .ToList();
    }
}
