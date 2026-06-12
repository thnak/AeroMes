using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Downtime.Queries.GetDowntimeLogs;

public class GetDowntimeLogsHandler(IDowntimeLogRepository repo)
    : IQueryHandler<GetDowntimeLogsQuery, IReadOnlyList<DowntimeLogDto>>
{
    public async Task<IReadOnlyList<DowntimeLogDto>> HandleAsync(GetDowntimeLogsQuery q, CancellationToken ct)
    {
        var logs = await repo.GetFilteredAsync(q.MachineCode, q.IsOpen, q.From, q.To, ct);

        return logs.Select(d => new DowntimeLogDto(
            d.DowntimeLogID,
            d.MachineCode,
            d.ReasonCode,
            d.ReasonName,
            d.StartTime,
            d.EndTime,
            d.DurationMinutes,
            d.OperatorID,
            d.Notes,
            d.EndTime == null)).ToList();
    }
}
