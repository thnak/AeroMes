using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.Queries.GetDHUTrend;

public class GetDHUTrendHandler(IInlineInspectionRepository repo)
    : IQueryHandler<GetDHUTrendQuery, IReadOnlyList<DHUTrendDto>>
{
    public Task<IReadOnlyList<DHUTrendDto>> HandleAsync(GetDHUTrendQuery query, CancellationToken ct)
        => repo.GetDHUTrendAsync(query.WOID, query.WorkCenterID, query.StyleCode, query.FromDate, query.ToDate, ct);
}
