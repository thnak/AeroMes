using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Search.Queries.GlobalSearch;

public record GlobalSearchQuery(
    string Q,
    string[]? Types,
    int Page,
    int PageSize,
    IEnumerable<string> AllowedPermissions) : IQuery<SearchResultPageDto>;
