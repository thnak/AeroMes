using AeroMes.Application.Search;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Microsoft.Extensions.Logging;

namespace AeroMes.Infrastructure.Search;

public class ElasticsearchIndexer(ElasticsearchClient client, ILogger<ElasticsearchIndexer> logger)
    : ISearchIndexer
{
    public async Task EnsureIndexAsync(string indexName, CancellationToken ct = default)
    {
        var exists = await client.Indices.ExistsAsync(indexName, ct);
        if (exists.Exists) return;

        var create = await client.Indices.CreateAsync(indexName, cfg => cfg
            .Mappings(m => m.Properties(new Properties
            {
                ["entity_type"] = new KeywordProperty(),
                ["title"] = new TextProperty { Analyzer = "standard" },
                ["subtitle"] = new KeywordProperty(),
                ["code"] = new KeywordProperty(),
                ["tags"] = new TextProperty { Analyzer = "standard" },
                ["description"] = new TextProperty { Analyzer = "standard" },
                ["required_permission"] = new KeywordProperty(),
            })), ct);

        if (!create.IsValidResponse)
            logger.LogWarning("Failed to create index {IndexName}: {Error}",
                indexName, create.ElasticsearchServerError?.Error?.Reason);
    }

    public async Task UpsertAsync(string indexName, string id, SearchDocument doc, CancellationToken ct = default)
    {
        var esDoc = ToEsDoc(doc);
        var response = await client.IndexAsync(esDoc, i => i.Index(indexName).Id(id), ct);
        if (!response.IsValidResponse)
            logger.LogWarning("Failed to upsert {Id} in {Index}: {Error}",
                id, indexName, response.ElasticsearchServerError?.Error?.Reason);
    }

    public async Task DeleteAsync(string indexName, string id, CancellationToken ct = default)
    {
        var response = await client.DeleteAsync(indexName, id, ct);
        if (!response.IsValidResponse && response.ElasticsearchServerError?.Status != 404)
            logger.LogWarning("Failed to delete {Id} from {Index}: {Error}",
                id, indexName, response.ElasticsearchServerError?.Error?.Reason);
    }

    public async Task BulkUpsertAsync(string indexName, IEnumerable<(string Id, SearchDocument Doc)> docs, CancellationToken ct = default)
    {
        foreach (var (id, doc) in docs)
        {
            var esDoc = ToEsDoc(doc);
            var response = await client.IndexAsync(esDoc, i => i.Index(indexName).Id(id), ct);
            if (!response.IsValidResponse)
                logger.LogWarning("Bulk upsert failed for {Id} in {Index}: {Error}",
                    id, indexName, response.ElasticsearchServerError?.Error?.Reason);
        }
    }

    private static EsDoc ToEsDoc(SearchDocument doc) =>
        new(doc.EntityType, doc.Title, doc.Subtitle, doc.Code,
            doc.Tags, doc.Description, doc.RequiredPermission);

    private record EsDoc(
        string EntityType,
        string Title,
        string? Subtitle,
        string? Code,
        string? Tags,
        string? Description,
        string? RequiredPermission);
}
