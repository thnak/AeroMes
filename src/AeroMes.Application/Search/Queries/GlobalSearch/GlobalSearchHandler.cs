using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Search.Queries.GlobalSearch;

public class GlobalSearchHandler(ISearchService searchService)
    : IQueryHandler<GlobalSearchQuery, SearchResultPageDto>
{
    public Task<SearchResultPageDto> HandleAsync(GlobalSearchQuery q, CancellationToken ct = default)
        => searchService.SearchAsync(q.Q, q.Types, q.Page, q.PageSize, q.AllowedPermissions, ct);
}
