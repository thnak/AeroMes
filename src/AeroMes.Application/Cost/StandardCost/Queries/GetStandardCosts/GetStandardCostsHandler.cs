using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.StandardCost.Queries.GetStandardCosts;

public sealed class GetStandardCostsHandler(IStandardCostRepository repo)
    : IQueryHandler<GetStandardCostsQuery, IReadOnlyList<StandardCostSummaryDto>>
{
    public Task<IReadOnlyList<StandardCostSummaryDto>> HandleAsync(
        GetStandardCostsQuery q, CancellationToken ct)
        => repo.GetListAsync(q.ProductCode, q.Status, ct);
}
