using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.CriteriaGroups.Queries.GetCriteriaGroups;

public record GetCriteriaGroupsQuery(string? Keyword = null, bool IncludeInactive = false)
    : IQuery<IReadOnlyList<QualityCriteriaGroupDto>>;
