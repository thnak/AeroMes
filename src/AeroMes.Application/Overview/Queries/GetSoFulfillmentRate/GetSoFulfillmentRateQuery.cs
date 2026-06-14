using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetSoFulfillmentRate;

public record GetSoFulfillmentRateQuery(int Year, int Month) : IQuery<SoFulfillmentDto>;

public class GetSoFulfillmentRateQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetSoFulfillmentRateQuery, SoFulfillmentDto>
{
    public Task<SoFulfillmentDto> HandleAsync(GetSoFulfillmentRateQuery query, CancellationToken ct = default)
        => repo.GetSoFulfillmentRateAsync(query.Year, query.Month, ct);
}
