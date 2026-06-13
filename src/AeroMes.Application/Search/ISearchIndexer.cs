namespace AeroMes.Application.Search;

public interface ISearchIndexer
{
    Task UpsertAsync(string indexName, string id, SearchDocument doc, CancellationToken ct = default);
    Task DeleteAsync(string indexName, string id, CancellationToken ct = default);
    Task BulkUpsertAsync(string indexName, IEnumerable<(string Id, SearchDocument Doc)> docs, CancellationToken ct = default);
    Task EnsureIndexAsync(string indexName, CancellationToken ct = default);
}
