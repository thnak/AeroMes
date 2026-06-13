using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.Queries.GetQualitySummaryByStyle;

public class GetQualitySummaryByStyleHandler(IInlineInspectionRepository repo)
    : IQueryHandler<GetQualitySummaryByStyleQuery, QualitySummaryByStyleDto?>
{
    public Task<QualitySummaryByStyleDto?> HandleAsync(GetQualitySummaryByStyleQuery query, CancellationToken ct)
        => repo.GetQualitySummaryAsync(query.StyleCode, query.FromDate, query.ToDate, ct);
}
