using AeroMes.Domain.Alert;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Alert.Queries.GetAlertEvents;

public record GetAlertEventsQuery(
    bool? IsActive = true,
    int? ThresholdId = null,
    int Page = 1,
    int PageSize = 50) : IQuery<IReadOnlyList<AlertEventDto>>;

public class GetAlertEventsHandler(IAlertEventRepository repo)
    : IQueryHandler<GetAlertEventsQuery, IReadOnlyList<AlertEventDto>>
{
    public Task<IReadOnlyList<AlertEventDto>> HandleAsync(GetAlertEventsQuery q, CancellationToken ct)
        => repo.GetListAsync(q.IsActive, q.ThresholdId, q.Page, q.PageSize, ct);
}
