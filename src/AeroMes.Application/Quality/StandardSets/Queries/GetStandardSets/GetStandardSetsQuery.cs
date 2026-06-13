using AeroMes.Application.Common;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.StandardSets.Queries.GetStandardSets;

public record GetStandardSetsQuery(
    string? Keyword, string? ProductCode, string? Status,
    int Page = 1, int PageSize = 20)
    : IQuery<PagedResult<StandardSetListDto>>;
