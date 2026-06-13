using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class WOCostRepository(AppDbContext db) : IWOCostRepository
{
    public Task<WOCostSummary?> GetSummaryByWOIDAsync(int woId, CancellationToken ct)
        => db.WOCostSummaries.FirstOrDefaultAsync(s => s.WOID == woId, ct);

    public async Task AddSummaryAsync(WOCostSummary summary, CancellationToken ct)
    {
        db.WOCostSummaries.Add(summary);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddMaterialLineAsync(WOMaterialCostLine line, CancellationToken ct)
    {
        db.WOMaterialCostLines.Add(line);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddLaborLineAsync(WOLaborCostLine line, CancellationToken ct)
    {
        db.WOLaborCostLines.Add(line);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddMachineLineAsync(WOMachineCostLine line, CancellationToken ct)
    {
        db.WOMachineCostLines.Add(line);
        await db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public async Task<WOCostSummaryDto?> GetSummaryDtoAsync(int woId, CancellationToken ct)
    {
        return await db.WOCostSummaries.AsNoTracking()
            .Where(s => s.WOID == woId)
            .Join(db.WorkOrders, s => s.WOID, wo => wo.WOID,
                (s, wo) => new WOCostSummaryDto(
                    s.WOCostID, s.WOID, wo.WOCode,
                    s.StdCostID, s.StdTotalCost,
                    s.ActMaterialCost, s.ActLaborCost,
                    s.ActMachineCost, s.ActMaintenanceCost,
                    s.ActMaterialCost + s.ActLaborCost + s.ActMachineCost + s.ActMaintenanceCost,
                    s.ActMaterialCost + s.ActLaborCost + s.ActMachineCost + s.ActMaintenanceCost - s.StdTotalCost,
                    s.ProducedQtyOK, s.ScrapQty,
                    s.VarianceDetailJson, s.UpdatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<WOMaterialCostLineDto>> GetMaterialLinesAsync(int woId, CancellationToken ct)
        => await db.WOMaterialCostLines.AsNoTracking()
            .Where(l => l.WOID == woId)
            .OrderByDescending(l => l.PostedAt)
            .Select(l => new WOMaterialCostLineDto(
                l.LineID, l.WOID, l.ConsumptionID,
                l.ProductCode, l.LotNumber,
                l.QtyConsumed, l.ActualUnitCost,
                l.QtyConsumed * l.ActualUnitCost, l.PostedAt))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<WOLaborCostLineDto>> GetLaborLinesAsync(int woId, CancellationToken ct)
        => await db.WOLaborCostLines.AsNoTracking()
            .Where(l => l.WOID == woId)
            .Join(db.LaborGrades, l => l.LaborGradeID, g => g.LaborGradeID,
                (l, g) => new WOLaborCostLineDto(
                    l.LineID, l.WOID, l.JobID,
                    l.OperatorID, l.LaborGradeID, g.GradeCode,
                    l.ActualHours, l.HourlyRateSnapshot,
                    l.IsOvertime, l.OvertimeMultiplierSnapshot,
                    l.ActualHours * l.HourlyRateSnapshot *
                        (1m + (l.IsOvertime ? l.OvertimeMultiplierSnapshot - 1m : 0m)),
                    l.PostedAt))
            .OrderByDescending(l => l.PostedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<WOMachineCostLineDto>> GetMachineLinesAsync(int woId, CancellationToken ct)
        => await db.WOMachineCostLines.AsNoTracking()
            .Where(l => l.WOID == woId)
            .OrderByDescending(l => l.PostedAt)
            .Select(l => new WOMachineCostLineDto(
                l.LineID, l.WOID, l.JobID,
                l.MachineCode, l.RuntimeHours, l.EnergyKWh,
                l.TotalRateSnapshot, l.RuntimeHours * l.TotalRateSnapshot, l.PostedAt))
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<VarianceReportItemDto> Items, int Total)> GetVarianceReportAsync(
        string? productCode, DateOnly? from, DateOnly? to, int? workCenterId,
        int page, int pageSize, CancellationToken ct)
    {
        var q = db.WOCostSummaries.AsNoTracking()
            .Join(db.WorkOrders, s => s.WOID, wo => wo.WOID, (s, wo) => new { s, wo })
            .Join(db.ProductionOrders, x => x.wo.POID, po => po.POID,
                (x, po) => new { x.s, x.wo, ProductCode = po.ProductCode });

        if (!string.IsNullOrEmpty(productCode))
            q = q.Where(x => x.ProductCode == productCode);
        if (workCenterId.HasValue)
            q = q.Where(x => x.wo.WorkCenterID == workCenterId.Value);
        if (from.HasValue)
            q = q.Where(x => DateOnly.FromDateTime(x.s.UpdatedAt) >= from.Value);
        if (to.HasValue)
            q = q.Where(x => DateOnly.FromDateTime(x.s.UpdatedAt) <= to.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(x => x.s.UpdatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new VarianceReportItemDto(
                x.s.WOID, x.wo.WOCode, x.ProductCode,
                x.s.StdTotalCost,
                x.s.ActMaterialCost + x.s.ActLaborCost + x.s.ActMachineCost + x.s.ActMaintenanceCost,
                x.s.ActMaterialCost + x.s.ActLaborCost + x.s.ActMachineCost + x.s.ActMaintenanceCost - x.s.StdTotalCost,
                x.s.StdTotalCost > 0
                    ? (x.s.ActMaterialCost + x.s.ActLaborCost + x.s.ActMachineCost + x.s.ActMaintenanceCost - x.s.StdTotalCost)
                        / x.s.StdTotalCost * 100m
                    : 0m,
                x.s.ProducedQtyOK,
                x.s.UpdatedAt))
            .ToListAsync(ct);

        return (items, total);
    }
}
