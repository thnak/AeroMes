using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.StandardSets.Queries.GetEffectiveStandardSet;

public class GetEffectiveStandardSetHandler(IQualityStandardSetRepository repository)
    : IQueryHandler<GetEffectiveStandardSetQuery, StandardSetDetailDto?>
{
    public Task<StandardSetDetailDto?> HandleAsync(
        GetEffectiveStandardSetQuery query, CancellationToken ct)
        => repository.GetEffectiveAsync(query.ProductCode, query.Date, ct);
}
