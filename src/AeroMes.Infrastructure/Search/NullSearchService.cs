using AeroMes.Application.Search;

namespace AeroMes.Infrastructure.Search;

public class NullSearchService : ISearchService
{
    public Task<SearchResultPageDto> SearchAsync(
        string query, string[]? types, int page, int pageSize,
        IEnumerable<string> allowedPermissions, CancellationToken ct = default)
        => Task.FromResult(new SearchResultPageDto(query, 0, page, pageSize, []));
}
