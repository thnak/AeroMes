using AeroMes.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Application.Production.Queries.GetOee;

public class GetOeeHandler(IApplicationDbContext db)
    : IRequestHandler<GetOeeQuery, OeeResult>
{
    public async Task<OeeResult> Handle(GetOeeQuery q, CancellationToken ct)
    {
        var totalPlanned = (q.ShiftEnd - q.ShiftStart).TotalMinutes;

        var downtimeLogs = await db.DowntimeLogs
            .Where(x =>
                x.WorkCenterID == q.WorkCenterId &&
                x.MachineCode == q.MachineCode &&
                x.StartTime >= q.ShiftStart &&
                (x.EndTime == null || x.EndTime <= q.ShiftEnd))
            .Select(x => new { x.StartTime, x.EndTime })
            .ToListAsync(ct);

        var downtimeMinutes = downtimeLogs.Sum(x =>
            x.EndTime.HasValue ? (x.EndTime.Value - x.StartTime).TotalMinutes : 0);

        var production = await db.ProductionLogs
            .Where(x =>
                x.MachineCode == q.MachineCode &&
                x.Timestamp >= q.ShiftStart &&
                x.Timestamp <= q.ShiftEnd)
            .GroupBy(_ => 1)
            .Select(g => new { TotalOk = g.Sum(x => x.QtyOK), TotalNg = g.Sum(x => x.QtyNG) })
            .FirstOrDefaultAsync(ct);

        var ok = production?.TotalOk ?? 0;
        var ng = production?.TotalNg ?? 0;
        var total = ok + ng;

        var availability = totalPlanned > 0
            ? (totalPlanned - downtimeMinutes) / totalPlanned
            : 0;

        var actualRunSeconds = (totalPlanned - downtimeMinutes) * 60;
        var performance = actualRunSeconds > 0
            ? Math.Min(1.0, (total * q.DesignedCycleTimeSeconds) / actualRunSeconds)
            : 0;

        var quality = total > 0 ? (double)ok / total : 0;
        var oee = availability * performance * quality;

        return new OeeResult(
            q.MachineCode,
            Math.Round(totalPlanned, 2),
            Math.Round(downtimeMinutes, 2),
            total, ok, ng,
            Math.Round(availability * 100, 2),
            Math.Round(performance * 100, 2),
            Math.Round(quality * 100, 2),
            Math.Round(oee * 100, 2));
    }
}
