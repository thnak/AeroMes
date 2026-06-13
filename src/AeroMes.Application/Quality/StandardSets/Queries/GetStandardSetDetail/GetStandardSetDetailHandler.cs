using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.StandardSets.Queries.GetStandardSetDetail;

public class GetStandardSetDetailHandler(IQualityStandardSetRepository repository)
    : IQueryHandler<GetStandardSetDetailQuery, StandardSetDetailDto?>
{
    public Task<StandardSetDetailDto?> HandleAsync(
        GetStandardSetDetailQuery query, CancellationToken ct)
        => repository.GetDetailAsync(query.StandardSetID, ct);
}
