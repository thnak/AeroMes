using AeroMes.Application.Search;

namespace AeroMes.Infrastructure.Search;

public class NullSearchIndexer : ISearchIndexer
{
    public Task UpsertAsync(string indexName, string id, SearchDocument doc, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task DeleteAsync(string indexName, string id, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task BulkUpsertAsync(string indexName, IEnumerable<(string Id, SearchDocument Doc)> docs, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task EnsureIndexAsync(string indexName, CancellationToken ct = default) =>
        Task.CompletedTask;
}
