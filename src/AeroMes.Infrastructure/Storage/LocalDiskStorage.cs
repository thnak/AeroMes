using AeroMes.Application.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AeroMes.Infrastructure.Storage;

/// <summary>
/// On-prem / offline-first <see cref="IFileStorage"/> that stores files under a configured
/// root directory. The default provider — no external service required.
/// </summary>
public sealed class LocalDiskStorage : IFileStorage
{
    private readonly FileStorageOptions _options;
    private readonly ILogger<LocalDiskStorage> _logger;
    private readonly string _root;

    public LocalDiskStorage(
        IOptions<FileStorageOptions> options,
        IHostEnvironment env,
        ILogger<LocalDiskStorage> logger)
    {
        _options = options.Value;
        _logger = logger;

        var configured = _options.Local.RootPath;
        _root = Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(env.ContentRootPath, configured);
        Directory.CreateDirectory(_root);
    }

    public async Task<StoredFileInfo> SaveAsync(FileSaveRequest request, CancellationToken ct = default)
    {
        FileStorageHelpers.Validate(request, _options);

        var key = FileStorageHelpers.BuildKey(request.Container, request.FileName);
        var spooled = await FileStorageHelpers.SpoolToTempAsync(request.Content, _options.MaxSizeBytes, ct);

        try
        {
            var dest = ResolvePath(key);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Move(spooled.TempPath, dest, overwrite: false);
        }
        catch
        {
            FileStorageHelpers.TryDelete(spooled.TempPath);
            throw;
        }

        _logger.LogDebug("Stored file {Key} ({Size} bytes) on local disk", key, spooled.SizeBytes);
        return new StoredFileInfo(key, request.FileName, request.ContentType, spooled.SizeBytes, spooled.Checksum);
    }

    public Task<FileDownload?> OpenReadAsync(string storageKey, CancellationToken ct = default)
    {
        var path = ResolvePath(storageKey);
        if (!File.Exists(path))
            return Task.FromResult<FileDownload?>(null);

        Stream stream = new FileStream(
            path, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 81920, useAsync: true);

        var fileName = Path.GetFileName(storageKey);
        var download = new FileDownload(stream, "application/octet-stream", stream.Length, fileName);
        return Task.FromResult<FileDownload?>(download);
    }

    public Task<bool> DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        var path = ResolvePath(storageKey);
        if (!File.Exists(path))
            return Task.FromResult(false);

        File.Delete(path);
        return Task.FromResult(true);
    }

    public Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default)
        => Task.FromResult(File.Exists(ResolvePath(storageKey)));

    public Task<string?> GetPresignedUrlAsync(string storageKey, TimeSpan expiry, CancellationToken ct = default)
    {
        // Local disk has no native presigning; expose a public URL only when a CDN/proxy is configured.
        var baseUrl = _options.Local.PublicBaseUrl;
        if (string.IsNullOrWhiteSpace(baseUrl))
            return Task.FromResult<string?>(null);

        return Task.FromResult<string?>($"{baseUrl.TrimEnd('/')}/{storageKey}");
    }

    /// <summary>Resolve a key to an absolute path, guarding against traversal outside the root.</summary>
    private string ResolvePath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
            throw new ArgumentException("Storage key is required.", nameof(storageKey));

        var combined = Path.GetFullPath(Path.Combine(_root, storageKey));
        var rootFull = Path.GetFullPath(_root);
        if (!combined.StartsWith(rootFull, StringComparison.Ordinal))
            throw new ArgumentException("Storage key resolves outside the storage root.", nameof(storageKey));

        return combined;
    }
}
