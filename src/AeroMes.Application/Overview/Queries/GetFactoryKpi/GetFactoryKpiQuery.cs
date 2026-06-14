using AeroMes.Application.Interfaces;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Overview.Queries.GetFactoryKpi;

public record GetFactoryKpiQuery(DateOnly Date) : IQuery<FactoryKpiDto>;

public class GetFactoryKpiQueryHandler(IOverviewRepository repo)
    : IQueryHandler<GetFactoryKpiQuery, FactoryKpiDto>
{
    public Task<FactoryKpiDto> HandleAsync(GetFactoryKpiQuery query, CancellationToken ct = default)
        => repo.GetFactoryKpiAsync(query.Date, ct);
}
