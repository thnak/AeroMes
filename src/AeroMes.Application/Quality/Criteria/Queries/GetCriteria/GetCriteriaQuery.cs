using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.Criteria.Queries.GetCriteria;

public record GetCriteriaQuery(string? Keyword, string? Status, int? GroupID)
    : IQuery<IReadOnlyList<QualityCriteriaDto>>;
