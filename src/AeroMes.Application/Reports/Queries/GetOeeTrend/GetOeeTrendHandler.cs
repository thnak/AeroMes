using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetOeeTrend;

public class GetOeeTrendHandler(
    IProductionLogRepository productionLogRepo,
    IDowntimeLogRepository downtimeLogRepo,
    IMachineRepository machineRepo)
    : IQueryHandler<GetOeeTrendQuery, OeeTrendDto>
{
    public async Task<OeeTrendDto> HandleAsync(GetOeeTrendQuery q, CancellationToken ct)
    {
        var machine = await machineRepo.GetByCodeAsync(q.MachineCode, ct);
        var capacityPerHour = (double)(machine?.TheoreticalCapacityPerHour ?? 0);
        var plannedDowntimePerShift = machine?.PlannedDowntimeMinPerShift ?? 0;

        var logs = await productionLogRepo.GetForReportAsync(q.From, q.To, null, q.MachineCode, ct);
        var downtimeLogs = await downtimeLogRepo.GetFilteredAsync(q.MachineCode, isOpen: false, q.From, q.To, ct);

        var periods = BuildPeriods(q.From, q.To, q.Granularity);
        var points = new List<OeeTrendPointDto>();

        foreach (var (label, start, end) in periods)
        {
            var periodLogs = logs.Where(l => l.Timestamp >= start && l.Timestamp < end).ToList();
            var ok = periodLogs.Sum(l => l.QtyOK);
            var ng = periodLogs.Sum(l => l.QtyNG);

            var downtimeMin = (double)downtimeLogs
                .Where(d => d.StartTime >= start && d.StartTime < end)
                .Sum(d => d.DurationMinutes ?? 0);

            var totalMinutes = (end - start).TotalMinutes;
            var workingMinutes = totalMinutes - plannedDowntimePerShift;
            var runMinutes = Math.Max(0, workingMinutes - downtimeMin);

            var availability = workingMinutes > 0 ? Math.Min(1.0, runMinutes / workingMinutes) : 0;
            var totalProduced = ok + ng;
            var theoreticalOutput = capacityPerHour > 0 ? (runMinutes / 60.0 * capacityPerHour) : 0;
            var performance = theoreticalOutput > 0
                ? Math.Min(1.0, totalProduced / theoreticalOutput) : 0;
            var quality = totalProduced > 0 ? (double)ok / totalProduced : 0;
            var oee = availability * performance * quality;

            points.Add(new OeeTrendPointDto(
                label, start, end,
                ok, ng,
                Math.Round((double)downtimeMin, 1),
                Math.Round(availability * 100, 1),
                Math.Round(quality * 100, 1),
                Math.Round(oee * 100, 1)));
        }

        return new OeeTrendDto(q.MachineCode, q.From, q.To, q.Granularity.ToString(), points);
    }

    private static List<(string Label, DateTime Start, DateTime End)> BuildPeriods(
        DateTime from, DateTime to, OeeTrendGranularity granularity)
    {
        var periods = new List<(string, DateTime, DateTime)>();
        var current = granularity switch
        {
            OeeTrendGranularity.Month => new DateTime(from.Year, from.Month, 1),
            OeeTrendGranularity.Week => from.AddDays(-(int)from.DayOfWeek),
            _ => from.Date,
        };

        while (current < to)
        {
            var next = granularity switch
            {
                OeeTrendGranularity.Month => current.AddMonths(1),
                OeeTrendGranularity.Week => current.AddDays(7),
                _ => current.AddDays(1),
            };
            var label = granularity switch
            {
                OeeTrendGranularity.Month => current.ToString("yyyy-MM"),
                OeeTrendGranularity.Week => $"W{System.Globalization.ISOWeek.GetWeekOfYear(current)}",
                _ => current.ToString("yyyy-MM-dd"),
            };
            periods.Add((label, current, next > to ? to : next));
            current = next;
        }
        return periods;
    }
}
