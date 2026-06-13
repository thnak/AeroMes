using AeroMes.Application.Common;
using AeroMes.Domain.Integration.Repositories;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetProductionOrderProgress;

public sealed class GetProductionOrderProgressHandler(
    IProductionOrderRepository poRepo,
    IProductionOrderProgressRepository progressRepo)
    : IQueryHandler<GetProductionOrderProgressQuery, QueryResult<ProductionOrderProgressDto>>
{
    public async Task<QueryResult<ProductionOrderProgressDto>> HandleAsync(
        GetProductionOrderProgressQuery query, CancellationToken ct)
    {
        var po = await poRepo.GetByIdAsync(query.POID, ct);
        if (po is null)
            return QueryResult<ProductionOrderProgressDto>.NotFound($"Production order #{query.POID} not found.");

        var data = await progressRepo.GetProgressDataAsync(query.POID, ct);
        if (data is null)
        {
            // PO exists but no work orders yet — return empty progress
            return QueryResult<ProductionOrderProgressDto>.Found(new ProductionOrderProgressDto(
                po.POID, po.POCode, po.ProductCode, po.TargetQuantity,
                po.Status.ToString(), po.PlannedStartDate, po.PlannedEndDate,
                0, 0, 0, 0, 0, 0, 100, [], [], [], [], []));
        }

        // ── Aggregate KPIs ───────────────────────────────────────────────────
        var totalOk = data.Logs.Sum(l => l.QtyOK);
        var totalNg = data.Logs.Sum(l => l.QtyNG);
        var overallPct = po.TargetQuantity > 0
            ? Math.Min(100, totalOk * 100.0 / po.TargetQuantity)
            : 0;
        var passRate = (totalOk + totalNg) > 0
            ? totalOk * 100.0 / (totalOk + totalNg)
            : 100;

        var completedJobs = data.Jobs.Where(j => j.EndTime.HasValue).ToList();
        var usedHours = completedJobs.Sum(j => (j.EndTime!.Value - j.StartTime).TotalHours);
        var plannedHours = po.PlannedStartDate.HasValue && po.PlannedEndDate.HasValue
            ? (po.PlannedEndDate.Value - po.PlannedStartDate.Value).TotalHours
            : 0;
        var timeUsagePct = plannedHours > 0 ? usedHours * 100.0 / plannedHours : 0;

        // ── Stages ───────────────────────────────────────────────────────────
        var handoversByFromWo = data.Handovers
            .Where(h => h.Status == "Submitted" || h.Status == "Draft")
            .Select(h => h.FromWOID)
            .ToHashSet();

        var logsByWo = data.Logs.ToLookup(l => l.WOID);

        var stages = data.WorkOrders.Select(wo =>
        {
            var woLogs = logsByWo[wo.WOID];
            var qtyOk = woLogs.Sum(l => l.QtyOK);
            var qtyNg = woLogs.Sum(l => l.QtyNG);
            return new StageProgressDto(
                wo.WOID, wo.WOCode, wo.StepNumber, wo.OperationCode, wo.WorkCenterName,
                wo.Status, wo.TargetQty, qtyOk, qtyNg,
                Math.Max(0, wo.TargetQty - qtyOk - qtyNg),
                wo.ActualStart, wo.ActualEnd,
                data.Jobs.Count(j => j.WOID == wo.WOID),
                handoversByFromWo.Contains(wo.WOID));
        }).ToList();

        // ── Output summaries ─────────────────────────────────────────────────
        var woCodeMap = data.WorkOrders.ToDictionary(w => w.WOID);
        var jobMap = data.Jobs.ToDictionary(j => j.JobID);

        var outputSummaries = data.Jobs.Select(j =>
        {
            var woRow = woCodeMap.TryGetValue(j.WOID, out var w) ? w : null;
            var jobLogs = data.Logs.Where(l => l.JobID == j.JobID).ToList();
            return new OutputSummaryDto(
                j.WOID, woRow?.WOCode ?? string.Empty, woRow?.OperationCode ?? string.Empty,
                j.JobID, j.MachineCode, j.ShiftCode, j.StartTime, j.EndTime,
                jobLogs.Sum(l => l.QtyOK), jobLogs.Sum(l => l.QtyNG));
        }).ToList();

        // ── Quality ──────────────────────────────────────────────────────────
        var qualityItems = data.WorkOrders.Select(wo =>
        {
            var woLogs = logsByWo[wo.WOID];
            var allDefects = woLogs.SelectMany(l => l.Defects).ToList();
            var breakdown = allDefects
                .GroupBy(d => d.DefectCodeId)
                .Select(g => new DefectSummaryDto(g.Key, g.Sum(d => d.Qty)))
                .ToList();
            return new QualityProgressDto(
                wo.WOID, wo.WOCode, wo.OperationCode,
                allDefects.Sum(d => d.Qty), breakdown.Count, breakdown);
        }).ToList();

        // ── Handovers ────────────────────────────────────────────────────────
        var handoverDtos = data.Handovers.Select(h => new HandoverProgressDto(
            h.FormID, h.FormNumber, h.FormType, h.Status,
            h.FromWOID, h.ToWOID, h.HandoverDate)).ToList();

        // ── Materials (BOM-based placeholder) ────────────────────────────────
        // Material consumption tracking requires movement records (#86).
        // Return empty list until material requisition flow is tracked per PO.
        List<MaterialProgressDto> materials = [];

        return QueryResult<ProductionOrderProgressDto>.Found(new ProductionOrderProgressDto(
            po.POID, po.POCode, po.ProductCode, po.TargetQuantity,
            po.Status.ToString(), po.PlannedStartDate, po.PlannedEndDate,
            totalOk, totalNg, overallPct, plannedHours, usedHours, timeUsagePct, passRate,
            stages, materials, outputSummaries, qualityItems, handoverDtos));
    }
}
