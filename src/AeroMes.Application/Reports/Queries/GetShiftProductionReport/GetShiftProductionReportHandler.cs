using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetShiftProductionReport;

public class GetShiftProductionReportHandler(
    IJobRepository jobRepo,
    IProductionLogRepository productionLogRepo,
    IDowntimeLogRepository downtimeLogRepo,
    IWorkOrderRepository workOrderRepo,
    IMachineRepository machineRepo)
    : IQueryHandler<GetShiftProductionReportQuery, ShiftProductionReportDto>
{
    public async Task<ShiftProductionReportDto> HandleAsync(GetShiftProductionReportQuery q, CancellationToken ct)
    {
        var from = q.ShiftDate.ToDateTime(TimeOnly.MinValue);
        var to = from.AddDays(1);

        var jobs = await jobRepo.GetFilteredAsync(null, null, null, from, to, ct);

        // Filter by shift code if provided
        var filteredJobs = q.ShiftCode is null
            ? jobs
            : jobs.Where(j => j.ShiftCode.Equals(q.ShiftCode, StringComparison.OrdinalIgnoreCase)).ToList();

        // Group by machine code
        var machineGroups = filteredJobs.GroupBy(j => j.MachineCode).ToList();

        var machines = await machineRepo.GetAllAsync(activeOnly: false, ct);
        var machineMap = machines.ToDictionary(m => m.MachineCode, m => m);

        // Filter by work center if requested
        if (q.WorkCenterId.HasValue)
        {
            machineGroups = machineGroups
                .Where(g => machineMap.TryGetValue(g.Key, out var m) && m.WorkCenterID == q.WorkCenterId)
                .ToList();
        }

        var rows = new List<ShiftProductionRowDto>();

        foreach (var group in machineGroups)
        {
            var machineCode = group.Key;
            machineMap.TryGetValue(machineCode, out var machine);

            var productionLogs = await productionLogRepo.GetForReportAsync(from, to, null, machineCode, ct);
            var ok = productionLogs.Sum(l => l.QtyOK);
            var ng = productionLogs.Sum(l => l.QtyNG);

            // Aggregate target from related work orders
            var woIds = group.Select(j => j.WOID).Distinct().ToList();
            var targetQty = 0;
            foreach (var woId in woIds)
            {
                var wo = await workOrderRepo.GetByIdAsync(woId, ct);
                if (wo is not null) targetQty += wo.TargetQuantity.Value;
            }

            var downtimeLogs = await downtimeLogRepo.GetFilteredAsync(machineCode, isOpen: false, from, to, ct);
            var downtimeMin = (double)downtimeLogs.Sum(d => d.DurationMinutes ?? 0);
            var topReason = downtimeLogs
                .GroupBy(d => d.ReasonCode)
                .OrderByDescending(g => g.Sum(d => d.DurationMinutes ?? 0))
                .FirstOrDefault()?.First().ReasonName;

            var completion = targetQty > 0 ? Math.Round((double)ok / targetQty * 100, 1) : 0;

            rows.Add(new ShiftProductionRowDto(
                machineCode,
                machine?.MachineName,
                machine?.WorkCenter?.WorkCenterName,
                targetQty, ok, ng,
                completion,
                Math.Round(downtimeMin, 1),
                topReason,
                group.Count()));
        }

        return new ShiftProductionReportDto(q.ShiftDate, q.ShiftCode, q.WorkCenterId,
            rows.OrderBy(r => r.MachineCode).ToList());
    }
}
