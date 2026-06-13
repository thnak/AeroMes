using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.CriteriaGroups.Queries.GetCriteriaGroups;

public class GetCriteriaGroupsHandler(IQualityCriteriaGroupRepository repository)
    : IQueryHandler<GetCriteriaGroupsQuery, IReadOnlyList<QualityCriteriaGroupDto>>
{
    public Task<IReadOnlyList<QualityCriteriaGroupDto>> HandleAsync(
        GetCriteriaGroupsQuery query, CancellationToken ct)
        => repository.GetListAsync(query.Keyword, query.IncludeInactive, ct);
}
