namespace AeroMes.Application.Storage;

public interface IFileStorage
{
    Task<string> SaveAsync(Stream stream, string key, string contentType, CancellationToken ct = default);
    Task<Stream> OpenReadAsync(string key, CancellationToken ct = default);
    Task DeleteAsync(string key, CancellationToken ct = default);
    Task<string> GetUrlAsync(string key, TimeSpan ttl, CancellationToken ct = default);
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);
}
