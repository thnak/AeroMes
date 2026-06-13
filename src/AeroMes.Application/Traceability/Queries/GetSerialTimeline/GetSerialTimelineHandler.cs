using AeroMes.Domain.Traceability.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Traceability.Queries.GetSerialTimeline;

public class GetSerialTimelineHandler(ISerialUnitRepository repo)
    : IQueryHandler<GetSerialTimelineQuery, IReadOnlyList<SerialEventDto>>
{
    public Task<IReadOnlyList<SerialEventDto>> HandleAsync(GetSerialTimelineQuery query, CancellationToken ct)
        => repo.GetTimelineAsync(query.SerialNumber, ct);
}
