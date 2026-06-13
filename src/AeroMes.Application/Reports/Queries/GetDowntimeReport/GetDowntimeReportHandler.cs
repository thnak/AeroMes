using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Reports.Queries.GetDowntimeReport;

public class GetDowntimeReportHandler(IDowntimeLogRepository downtimeLogRepo)
    : IQueryHandler<GetDowntimeReportQuery, DowntimeReportDto>
{
    public async Task<DowntimeReportDto> HandleAsync(GetDowntimeReportQuery q, CancellationToken ct)
    {
        var logs = await downtimeLogRepo.GetFilteredAsync(q.MachineCode, isOpen: false, q.From, q.To, ct);

        var filtered = q.ReasonCode is null
            ? logs
            : logs.Where(l => l.ReasonCode == q.ReasonCode.ToUpperInvariant()).ToList();

        var rows = filtered
            .GroupBy(l => new { l.MachineCode, l.ReasonCode, l.ReasonName })
            .Select(g => new DowntimeReportRowDto(
                g.Key.MachineCode,
                g.Key.ReasonCode,
                g.Key.ReasonName,
                g.Count(),
                Math.Round((double)(g.Sum(l => l.DurationMinutes ?? 0)), 2)))
            .OrderByDescending(r => r.TotalMinutes)
            .ToList();

        return new DowntimeReportDto(
            q.From, q.To,
            rows.Sum(r => r.EventCount),
            Math.Round(rows.Sum(r => r.TotalMinutes), 2),
            rows);
    }
}
