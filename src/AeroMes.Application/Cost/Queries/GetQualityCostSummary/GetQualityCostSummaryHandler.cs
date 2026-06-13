using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.Queries.GetQualityCostSummary;

public class GetQualityCostSummaryHandler(IQualityCostSummaryRepository repository)
    : IQueryHandler<GetQualityCostSummaryQuery, IReadOnlyList<QualityCostSummaryDto>>
{
    public Task<IReadOnlyList<QualityCostSummaryDto>> HandleAsync(GetQualityCostSummaryQuery query, CancellationToken ct)
        => repository.GetSummaryAsync(query.Year, query.Month, query.ProductCode, ct);
}
