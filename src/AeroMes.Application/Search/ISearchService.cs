namespace AeroMes.Application.Search;

public interface ISearchService
{
    Task<SearchResultPageDto> SearchAsync(
        string query,
        string[]? types,
        int page,
        int pageSize,
        IEnumerable<string> allowedPermissions,
        CancellationToken ct = default);
}
