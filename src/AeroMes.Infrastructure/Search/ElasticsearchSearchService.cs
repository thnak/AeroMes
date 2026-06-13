using AeroMes.Application.Search;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Search;

public class ElasticsearchSearchService(ElasticsearchClient client, ILogger<ElasticsearchSearchService> logger)
    : ISearchService
{
    public async Task<SearchResultPageDto> SearchAsync(
        string query, string[]? types, int page, int pageSize,
        IEnumerable<string> allowedPermissions, CancellationToken ct = default)
    {
        var permSet = allowedPermissions.ToHashSet();
        var indices = types is { Length: > 0 }
            ? types.Select(t => $"aeromes_{t}s").ToArray()
            : SearchIndexNames.All;

        var from = (page - 1) * pageSize;

        var response = await client.SearchAsync<EsDoc>(s => s
            .Indices(indices)
            .From(from)
            .Size(pageSize)
            .Query(q => q.MultiMatch(mm => mm
                .Query(query)
                .Fields(new[] { "title^3", "code^2", "tags", "description", "subtitle" })
                .Fuzziness(new Fuzziness("AUTO")))), ct);

        if (!response.IsValidResponse)
        {
            logger.LogWarning("Search failed for query '{Query}': {Error}",
                query, response.ElasticsearchServerError?.Error?.Reason);
            return new SearchResultPageDto(query, 0, page, pageSize, []);
        }

        var results = new List<SearchResultDto>();
        foreach (var hit in response.Hits)
        {
            if (hit.Source is null) continue;
            if (hit.Source.RequiredPermission is not null && !permSet.Contains(hit.Source.RequiredPermission))
                continue;

            results.Add(new SearchResultDto(
                hit.Source.EntityType,
                hit.Id ?? string.Empty,
                hit.Source.Title,
                hit.Source.Subtitle,
                hit.Source.Code,
                []));
        }

        return new SearchResultPageDto(query, (int)response.Total, page, pageSize, results);
    }

    private record EsDoc(
        string EntityType, string Title, string? Subtitle,
        string? Code, string? Tags, string? Description, string? RequiredPermission);
}
