using AeroMes.Domain.Cost.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Cost.Queries.GetScrapPareto;

public class GetScrapParetoHandler(IScrapTransactionRepository repository)
    : IQueryHandler<GetScrapParetoQuery, IReadOnlyList<ScrapParetoDto>>
{
    public Task<IReadOnlyList<ScrapParetoDto>> HandleAsync(GetScrapParetoQuery query, CancellationToken ct)
        => repository.GetParetoAsync(query.From, query.To, query.WorkCenterId, ct);
}
