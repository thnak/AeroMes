using AeroMes.Application.Storage;
using Microsoft.Extensions.Options;

namespace AeroMes.Infrastructure.Storage;

public sealed class StorageOptions
{
    public string Provider { get; set; } = "Local";
    public LocalStorageOptions Local { get; set; } = new();
    public S3StorageOptions S3 { get; set; } = new();
}

public sealed class LocalStorageOptions
{
    public string RootPath { get; set; } = "uploads";
    public string BaseUrl { get; set; } = "/api/v1/files/serve";
}

public sealed class LocalDiskStorage(IOptions<StorageOptions> opts) : IFileStorage
{
    private readonly string _root = Path.GetFullPath(opts.Value.Local.RootPath);
    private readonly string _baseUrl = opts.Value.Local.BaseUrl;

    private string FullPath(string key) =>
        Path.Combine(_root, key.Replace('/', Path.DirectorySeparatorChar));

    public async Task<string> SaveAsync(Stream stream, string key, string contentType, CancellationToken ct)
    {
        var path = FullPath(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await using var fs = File.Create(path);
        await stream.CopyToAsync(fs, ct);
        return key;
    }

    public Task<Stream> OpenReadAsync(string key, CancellationToken ct)
    {
        var path = FullPath(key);
        if (!File.Exists(path)) throw new FileNotFoundException($"File not found: {key}");
        return Task.FromResult<Stream>(File.OpenRead(path));
    }

    public Task DeleteAsync(string key, CancellationToken ct)
    {
        var path = FullPath(key);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    public Task<string> GetUrlAsync(string key, TimeSpan ttl, CancellationToken ct) =>
        Task.FromResult($"{_baseUrl}/{Uri.EscapeDataString(key)}");

    public Task<bool> ExistsAsync(string key, CancellationToken ct) =>
        Task.FromResult(File.Exists(FullPath(key)));
}
