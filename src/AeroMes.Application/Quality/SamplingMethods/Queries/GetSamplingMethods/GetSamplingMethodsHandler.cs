using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.SamplingMethods.Queries.GetSamplingMethods;

public class GetSamplingMethodsHandler(ISamplingMethodRepository repo)
    : IQueryHandler<GetSamplingMethodsQuery, IReadOnlyList<SamplingMethodDto>>
{
    public Task<IReadOnlyList<SamplingMethodDto>> HandleAsync(
        GetSamplingMethodsQuery query, CancellationToken ct)
        => repo.GetListAsync(query.ActiveOnly, ct);
}
