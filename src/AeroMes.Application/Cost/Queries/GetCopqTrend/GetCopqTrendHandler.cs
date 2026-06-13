using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.Queries.GetCopqTrend;

public class GetCopqTrendHandler(IQualityCostSummaryRepository repository)
    : IQueryHandler<GetCopqTrendQuery, IReadOnlyList<CopqTrendPointDto>>
{
    public Task<IReadOnlyList<CopqTrendPointDto>> HandleAsync(GetCopqTrendQuery query, CancellationToken ct)
        => repository.GetCopqTrendAsync(query.Months, query.ProductCode, ct);
}
