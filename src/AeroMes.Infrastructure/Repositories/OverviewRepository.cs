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

    // ── Dashboard queries ──────────────────────────────────────────────────────

    public async Task<FactoryKpiDto> GetFactoryKpiAsync(DateOnly date, CancellationToken ct)
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd   = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var activeWOs = await db.WorkOrders.AsNoTracking()
            .CountAsync(w => w.Status == WorkOrderStatus.Running ||
                             w.Status == WorkOrderStatus.Paused, ct);

        var todayWOs = await db.WorkOrders.AsNoTracking()
            .Where(w => w.ActualStartDate >= dayStart && w.ActualStartDate <= dayEnd)
            .Select(w => new { w.TargetQuantity, w.ActualQtyOK, w.ActualQtyNG })
            .ToListAsync(ct);

        var totalTarget = todayWOs.Sum(w => (int)w.TargetQuantity.Value);
        var totalOK     = todayWOs.Sum(w => (int)w.ActualQtyOK.Value);
        var totalNG     = todayWOs.Sum(w => (int)w.ActualQtyNG.Value);
        var achievement = totalTarget > 0 ? Math.Round((double)totalOK / totalTarget * 100, 1) : 0.0;

        var openDowntime = await db.DowntimeLogs.AsNoTracking()
            .CountAsync(d => d.EndTime == null, ct);

        var totalDowntimeMin = await db.DowntimeLogs.AsNoTracking()
            .Where(d => d.StartTime >= dayStart && d.StartTime <= dayEnd && d.EndTime != null)
            .SumAsync(d => (double?)EF.Functions.DateDiffMinute(d.StartTime, d.EndTime!.Value) ?? 0, ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expiryLimit = today.AddDays(30);

        var lowStockCount = await db.InventoryStocks.AsNoTracking()
            .CountAsync(s => (s.Quantity - s.ReservedQty) < 10m, ct);

        var expiringCount = await db.InventoryStocks.AsNoTracking()
            .CountAsync(s => s.ExpiryDate != null &&
                             s.ExpiryDate <= expiryLimit &&
                             s.ExpiryDate >= today, ct);

        return new FactoryKpiDto(
            activeWOs, totalTarget, totalOK, totalNG, achievement,
            openDowntime, totalDowntimeMin, lowStockCount, expiringCount);
    }

    public async Task<IReadOnlyList<OeeByMachineDto>> GetOeeByMachineAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var fromDt = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toDt   = to.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        int days = to.DayNumber - from.DayNumber + 1;

        // Planned time: 8 hours/day as default when no calendar configured
        const double defaultShiftHoursPerDay = 8.0;
        double plannedMinPerMachine = days * defaultShiftHoursPerDay * 60.0;

        var machines = await db.Jobs.AsNoTracking()
            .Where(j => j.StartTime >= fromDt && j.StartTime <= toDt)
            .Select(j => j.MachineCode)
            .Distinct()
            .ToListAsync(ct);

        var result = new List<OeeByMachineDto>();
        foreach (var machine in machines)
        {
            var downtimeMin = await db.DowntimeLogs.AsNoTracking()
                .Where(d => d.MachineCode == machine && d.StartTime >= fromDt && d.EndTime != null)
                .SumAsync(d => (double?)EF.Functions.DateDiffMinute(d.StartTime, d.EndTime!.Value) ?? 0, ct);

            var output = await db.ProductionLogs.AsNoTracking()
                .Where(pl => pl.Timestamp >= fromDt && pl.Timestamp <= toDt)
                .Join(db.Jobs, pl => pl.JobID, j => j.JobID, (pl, j) => new { pl.QtyOK, pl.QtyNG, j.MachineCode })
                .Where(x => x.MachineCode == machine)
                .GroupBy(_ => 1)
                .Select(g => new { QtyOK = g.Sum(x => x.QtyOK), QtyNG = g.Sum(x => x.QtyNG) })
                .FirstOrDefaultAsync(ct);

            var qtyOK = output?.QtyOK ?? 0;
            var qtyNG = output?.QtyNG ?? 0;
            var totalParts = qtyOK + qtyNG;

            var availPct = plannedMinPerMachine > 0
                ? Math.Min(100.0, Math.Round((plannedMinPerMachine - downtimeMin) / plannedMinPerMachine * 100, 1))
                : 0.0;
            var qualityPct = totalParts > 0
                ? Math.Round((double)qtyOK / totalParts * 100, 1)
                : 100.0;
            var performancePct = 85.0; // default — requires cycle rate config
            var oee = Math.Round(availPct * performancePct * qualityPct / 10_000.0, 1);

            result.Add(new OeeByMachineDto(machine, availPct, performancePct, qualityPct, oee));
        }
        return result;
    }

    public async Task<IReadOnlyList<ShiftOutputDto>> GetShiftOutputSummaryAsync(DateOnly date, CancellationToken ct)
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd   = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var jobs = await db.Jobs.AsNoTracking()
            .Where(j => j.StartTime >= dayStart && j.StartTime <= dayEnd)
            .ToListAsync(ct);

        var logsByJob = await db.ProductionLogs.AsNoTracking()
            .Where(pl => pl.Timestamp >= dayStart && pl.Timestamp <= dayEnd)
            .GroupBy(pl => pl.JobID)
            .Select(g => new { JobID = g.Key, QtyOK = g.Sum(x => x.QtyOK), QtyNG = g.Sum(x => x.QtyNG) })
            .ToListAsync(ct);

        var logMap = logsByJob.ToDictionary(x => x.JobID);

        return jobs
            .GroupBy(j => j.ShiftCode)
            .Select(g =>
            {
                var ok = g.Sum(j => logMap.TryGetValue(j.JobID, out var l) ? l.QtyOK : 0);
                var ng = g.Sum(j => logMap.TryGetValue(j.JobID, out var l) ? l.QtyNG : 0);
                var total = ok + ng;
                return new ShiftOutputDto(
                    g.Key,
                    ok,
                    ng,
                    total > 0 ? Math.Round((double)ok / total * 100, 1) : 100.0,
                    g.Count(j => j.Status == JobStatus.Active),
                    g.Count(j => j.Status == JobStatus.Finished));
            })
            .OrderBy(x => x.ShiftCode)
            .ToList();
    }

    public async Task<IReadOnlyList<DefectParetoItemDto>> GetDefectParetoAsync(
        DateOnly from, DateOnly to, string? productCode, CancellationToken ct)
    {
        var fromDt = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toDt   = to.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var query = db.DefectDetails.AsNoTracking()
            .Include(d => d.DefectCode)
            .Join(db.ProductionLogs, d => d.LogID, pl => pl.LogID, (d, pl) => new { d, pl })
            .Where(x => x.pl.Timestamp >= fromDt && x.pl.Timestamp <= toDt);

        if (!string.IsNullOrEmpty(productCode))
        {
            query = query.Join(db.Jobs, x => x.pl.JobID, j => j.JobID, (x, j) => new { x.d, x.pl, j })
                .Join(db.WorkOrders, x => x.j.WOID, w => w.WOID, (x, w) => new { x.d, x.pl, w })
                .Join(db.ProductionOrders, x => x.w.POID, po => po.POID, (x, po) => new { x.d, x.pl, po })
                .Where(x => x.po.ProductCode == productCode)
                .Select(x => new { x.d, x.pl });
        }

        var raw = await query
            .Select(x => new
            {
                Code     = x.d.DefectCode!.Code,
                Name     = x.d.DefectCode!.DefectName,
                Category = x.d.DefectCode!.DefectCategory ?? "Uncategorized",
                Severity = x.d.DefectCode!.SeverityLevel.ToString(),
            })
            .ToListAsync(ct);

        var grouped = raw
            .GroupBy(x => new { x.Code, x.Name, x.Category, x.Severity })
            .Select(g => new { g.Key.Code, g.Key.Name, g.Key.Category, g.Key.Severity, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        var grandTotal = grouped.Sum(x => x.Count);
        double cumulative = 0;
        return grouped.Select(x =>
        {
            cumulative += grandTotal > 0 ? (double)x.Count / grandTotal * 100 : 0;
            return new DefectParetoItemDto(x.Code, x.Name, x.Category, x.Severity, x.Count, Math.Round(cumulative, 1));
        }).ToList();
    }

    public async Task<InventoryAlertSummaryDto> GetInventoryAlertsAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expiryLimit = today.AddDays(30);

        var lowStock = await db.InventoryStocks.AsNoTracking()
            .Include(s => s.StorageLocation)
            .Where(s => (s.Quantity - s.ReservedQty) < 10m)
            .Select(s => new LowStockAlertDto(
                s.ProductCode,
                s.Quantity - s.ReservedQty,
                s.StorageLocation != null ? s.StorageLocation.LocationCode : "N/A"))
            .Take(50)
            .ToListAsync(ct);

        var expiring = await db.InventoryStocks.AsNoTracking()
            .Where(s => s.ExpiryDate != null && s.ExpiryDate <= expiryLimit && s.ExpiryDate >= today)
            .Select(s => new
            {
                s.ProductCode, s.LotNumber, s.ExpiryDate,
                Qty = s.Quantity - s.ReservedQty,
            })
            .Take(50)
            .ToListAsync(ct);

        var expiringDtos = expiring
            .Select(s => new ExpiringLotAlertDto(
                s.ProductCode, s.LotNumber, s.ExpiryDate!.Value, s.Qty,
                s.ExpiryDate!.Value.DayNumber - today.DayNumber))
            .OrderBy(x => x.DaysUntilExpiry)
            .ToList();

        return new InventoryAlertSummaryDto(lowStock, expiringDtos);
    }

    public async Task<SoFulfillmentDto> GetSoFulfillmentRateAsync(int year, int month, CancellationToken ct)
    {
        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd   = monthStart.AddMonths(1).AddTicks(-1);
        var now        = DateTime.UtcNow;

        var orders = await db.SalesOrders.AsNoTracking()
            .Where(so => so.DeliveryDate != null &&
                         so.DeliveryDate >= monthStart &&
                         so.DeliveryDate <= monthEnd)
            .Select(so => new
            {
                so.SOCode,
                so.CustomerName,
                so.DeliveryDate,
                so.Status,
            })
            .ToListAsync(ct);

        var total = orders.Count;
        var onTime = orders.Count(o =>
            o.Status == SalesOrderStatus.Closed &&
            o.DeliveryDate >= monthStart);
        var overdue = orders.Count(o =>
            o.Status != SalesOrderStatus.Closed &&
            o.Status != SalesOrderStatus.Cancelled &&
            o.DeliveryDate < now);
        var inProd = orders.Count(o => o.Status == SalesOrderStatus.InProduction);

        var topOverdue = orders
            .Where(o => o.Status != SalesOrderStatus.Closed &&
                        o.Status != SalesOrderStatus.Cancelled &&
                        o.DeliveryDate < now)
            .OrderByDescending(o => (now - o.DeliveryDate!.Value).TotalDays)
            .Take(10)
            .Select(o => new OverdueSoDto(
                o.SOCode,
                o.CustomerName,
                o.DeliveryDate!.Value,
                (int)(now - o.DeliveryDate!.Value).TotalDays))
            .ToList();

        return new SoFulfillmentDto(
            total, onTime, overdue, inProd,
            total > 0 ? Math.Round((double)onTime / total * 100, 1) : 100.0,
            topOverdue);
    }

    public async Task<MyDailyOutputDto> GetMyDailyOutputAsync(string operatorId, DateOnly date, CancellationToken ct)
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd   = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var jobs = await db.Jobs.AsNoTracking()
            .Include(j => j.WorkOrder)
            .Where(j => j.OperatorID == operatorId &&
                        j.StartTime >= dayStart && j.StartTime <= dayEnd)
            .ToListAsync(ct);

        var jobIds = jobs.Select(j => j.JobID).ToList();

        var logs = await db.ProductionLogs.AsNoTracking()
            .Where(pl => jobIds.Contains(pl.JobID) && pl.Timestamp >= dayStart)
            .GroupBy(pl => pl.JobID)
            .Select(g => new { JobID = g.Key, QtyOK = g.Sum(x => x.QtyOK), QtyNG = g.Sum(x => x.QtyNG) })
            .ToListAsync(ct);

        var logMap = logs.ToDictionary(x => x.JobID);

        int totalOK  = logMap.Values.Sum(l => l.QtyOK);
        int totalNG  = logMap.Values.Sum(l => l.QtyNG);
        int total    = totalOK + totalNG;

        var downtimeMin = await db.DowntimeLogs.AsNoTracking()
            .Where(d => d.StartTime >= dayStart && d.StartTime <= dayEnd &&
                        jobs.Select(j => j.MachineCode).Contains(d.MachineCode) &&
                        d.EndTime != null)
            .SumAsync(d => (double?)EF.Functions.DateDiffMinute(d.StartTime, d.EndTime!.Value) ?? 0, ct);

        var jobSummaries = jobs.Select(j =>
        {
            logMap.TryGetValue(j.JobID, out var l);
            return new MyJobSummaryDto(
                j.JobID, j.WorkOrder?.WOCode ?? $"WO-{j.WOID}",
                j.MachineCode, j.ShiftCode,
                l?.QtyOK ?? 0, l?.QtyNG ?? 0,
                j.StartTime, j.EndTime);
        }).ToList();

        return new MyDailyOutputDto(
            operatorId,
            totalOK, totalNG,
            total > 0 ? Math.Round((double)totalOK / total * 100, 1) : 100.0,
            jobs.Count(j => j.Status == JobStatus.Finished),
            jobs.Count(j => j.Status == JobStatus.Active),
            downtimeMin,
            jobSummaries);
    }

    public async Task<IReadOnlyList<MyDailyOutputDto>> GetMyOutputHistoryAsync(
        string operatorId, int days, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = new List<MyDailyOutputDto>(days);
        for (int i = days - 1; i >= 0; i--)
        {
            var d = today.AddDays(-i);
            result.Add(await GetMyDailyOutputAsync(operatorId, d, ct));
        }
        return result;
    }
}
