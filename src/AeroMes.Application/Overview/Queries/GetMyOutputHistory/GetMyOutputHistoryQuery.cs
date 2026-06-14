using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetMyOutputHistory;

public record GetMyOutputHistoryQuery(string OperatorId, int Days = 30) : IQuery<IReadOnlyList<MyDailyOutputDto>>;

public class GetMyOutputHistoryQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetMyOutputHistoryQuery, IReadOnlyList<MyDailyOutputDto>>
{
    public Task<IReadOnlyList<MyDailyOutputDto>> HandleAsync(GetMyOutputHistoryQuery query, CancellationToken ct = default)
        => repo.GetMyOutputHistoryAsync(query.OperatorId, query.Days, ct);
}
