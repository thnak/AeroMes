using AeroMes.Domain.Production.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Repositories;

public class ProductionOrderProgressRepository(AppDbContext db) : IProductionOrderProgressRepository
{
    public async Task<PoProgressData?> GetProgressDataAsync(int poId, CancellationToken ct)
    {
        var workOrders = await db.WorkOrders.AsNoTracking()
            .Where(w => w.POID == poId)
            .Include(w => w.RoutingStep).ThenInclude(rs => rs!.Operation)
            .Include(w => w.WorkCenter)
            .OrderBy(w => w.RoutingStep!.StepNumber)
            .ToListAsync(ct);

        if (workOrders.Count == 0)
            return null;

        var woIds = workOrders.Select(w => w.WOID).ToList();

        var jobs = await db.Jobs.AsNoTracking()
            .Where(j => woIds.Contains(j.WOID))
            .OrderBy(j => j.StartTime)
            .ToListAsync(ct);

        var jobIds = jobs.Select(j => j.JobID).ToList();

        var logs = await db.ProductionLogs.AsNoTracking()
            .Where(l => jobIds.Contains(l.JobID))
            .Include(l => l.DefectDetails)
            .ToListAsync(ct);

        var handovers = await db.StageHandoverForms.AsNoTracking()
            .Where(h => woIds.Contains(h.FromWorkOrderID) || woIds.Contains(h.ToWorkOrderID))
            .OrderBy(h => h.HandoverDate)
            .ToListAsync(ct);

        var jobWoMap = jobs.ToDictionary(j => j.JobID, j => j.WOID);

        var woRows = workOrders.Select(w => new WoProgressRow(
            w.WOID, w.WOCode, w.POID, w.RoutingStepID,
            w.RoutingStep?.StepNumber ?? 0,
            w.RoutingStep?.OperationCode ?? string.Empty,
            w.WorkCenter?.WorkCenterName ?? string.Empty,
            w.Status.ToString(),
            w.TargetQuantity.Value,
            w.ActualQtyOK.Value,
            w.ActualQtyNG.Value,
            w.ActualStartDate,
            w.ActualEndDate)).ToList();

        var jobRows = jobs.Select(j => new JobProgressRow(
            j.JobID, j.WOID, j.MachineCode, j.ShiftCode,
            j.StartTime, j.EndTime, j.Status.ToString())).ToList();

        var logRows = logs.Select(l => new LogProgressRow(
            l.LogID, l.JobID,
            jobWoMap.TryGetValue(l.JobID, out var woid) ? woid : 0,
            l.QtyOK, l.QtyNG,
            l.DefectDetails.Select(d => (d.DefectCodeID, d.Quantity)).ToList())).ToList();

        var handoverRows = handovers.Select(h => new HandoverRow(
            h.FormID, h.FormNumber, h.FormType.ToString(), h.Status.ToString(),
            h.FromWorkOrderID, h.ToWorkOrderID, h.HandoverDate)).ToList();

        return new PoProgressData(woRows, jobRows, logRows, handoverRows);
    }
}
