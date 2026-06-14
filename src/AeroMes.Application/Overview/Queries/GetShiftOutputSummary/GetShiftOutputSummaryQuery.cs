using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetShiftOutputSummary;

public record GetShiftOutputSummaryQuery(DateOnly Date) : IQuery<IReadOnlyList<ShiftOutputDto>>;

public class GetShiftOutputSummaryQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetShiftOutputSummaryQuery, IReadOnlyList<ShiftOutputDto>>
{
    public Task<IReadOnlyList<ShiftOutputDto>> HandleAsync(GetShiftOutputSummaryQuery query, CancellationToken ct = default)
        => repo.GetShiftOutputSummaryAsync(query.Date, ct);
}
