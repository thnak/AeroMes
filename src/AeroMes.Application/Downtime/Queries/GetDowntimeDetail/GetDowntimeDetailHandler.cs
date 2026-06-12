using AeroMes.Application.Downtime.Queries.GetDowntimeLogs;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Downtime.Queries.GetDowntimeDetail;

public class GetDowntimeDetailHandler(IDowntimeLogRepository repo)
    : IQueryHandler<GetDowntimeDetailQuery, DowntimeLogDto>
{
    public async Task<DowntimeLogDto> HandleAsync(GetDowntimeDetailQuery q, CancellationToken ct)
    {
        var d = await repo.GetByIdAsync(q.Id, ct)
            ?? throw new EntityNotFoundException("DowntimeLog", q.Id);

        return new DowntimeLogDto(
            d.DowntimeLogID,
            d.MachineCode,
            d.ReasonCode,
            d.ReasonName,
            d.StartTime,
            d.EndTime,
            d.DurationMinutes,
            d.OperatorID,
            d.Notes,
            d.EndTime == null);
    }
}
