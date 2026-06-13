using AeroMes.Application.Downtime.Queries.GetDowntimeLogs;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Downtime.Queries.GetDowntimeDetail;

public class GetDowntimeDetailHandler(IDowntimeLogRepository repo)
    : IQueryHandler<GetDowntimeDetailQuery, QueryResult<DowntimeLogDto>>
{
    public async Task<QueryResult<DowntimeLogDto>> HandleAsync(GetDowntimeDetailQuery q, CancellationToken ct)
    {
        var d = await repo.GetByIdAsync(q.Id, ct);
        if (d is null) return QueryResult<DowntimeLogDto>.NotFound($"DowntimeLog '{q.Id}' was not found.");

        return QueryResult<DowntimeLogDto>.Found(new DowntimeLogDto(
            d.DowntimeLogID,
            d.MachineCode,
            d.ReasonCode,
            d.ReasonName,
            d.StartTime,
            d.EndTime,
            d.DurationMinutes,
            d.OperatorID,
            d.Notes,
            d.EndTime == null));
    }
}
