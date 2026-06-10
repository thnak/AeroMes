using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetOee;

public class GetOeeHandler(
    IProductionLogRepository productionLogRepo,
    IDowntimeLogRepository downtimeLogRepo)
    : IQueryHandler<GetOeeQuery, OeeResult>
{
    public async Task<OeeResult> HandleAsync(GetOeeQuery q, CancellationToken ct)
    {
        var totalPlanned = (q.ShiftEnd - q.ShiftStart).TotalMinutes;

        var downtimeMinutes = await downtimeLogRepo.GetTotalDowntimeMinutesAsync(
            q.MachineCode, q.ShiftStart, q.ShiftEnd, ct);

        var (ok, ng) = await productionLogRepo.GetTotalOutputByMachineAsync(
            q.MachineCode, q.ShiftStart, q.ShiftEnd, ct);

        var total = ok + ng;
        var actualRunSeconds = (totalPlanned - downtimeMinutes) * 60;

        var availability = totalPlanned > 0
            ? (totalPlanned - downtimeMinutes) / totalPlanned : 0;
        var performance = actualRunSeconds > 0
            ? Math.Min(1.0, total * q.DesignedCycleTimeSeconds / actualRunSeconds) : 0;
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
