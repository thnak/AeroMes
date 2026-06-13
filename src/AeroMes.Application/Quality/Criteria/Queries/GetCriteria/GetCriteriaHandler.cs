using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.Criteria.Queries.GetCriteria;

public class GetCriteriaHandler(IQualityCriteriaRepository repository)
    : IQueryHandler<GetCriteriaQuery, IReadOnlyList<QualityCriteriaDto>>
{
    public Task<IReadOnlyList<QualityCriteriaDto>> HandleAsync(GetCriteriaQuery query, CancellationToken ct)
        => repository.GetListAsync(query.Keyword, query.Status, query.GroupID, ct);
}
