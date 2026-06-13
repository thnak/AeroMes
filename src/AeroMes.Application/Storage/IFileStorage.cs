namespace AeroMes.Application.Storage;

/// <summary>
/// Provider-agnostic blob/file storage. Implementations live in Infrastructure
/// (local disk by default, S3/MinIO when configured). Callers reference files by
/// their opaque <see cref="StoredFileInfo.StorageKey"/> — never by physical path.
/// </summary>
public interface IFileStorage
{
    /// <summary>Persist a file and return its metadata (key + SHA-256 checksum + size).</summary>
    Task<StoredFileInfo> SaveAsync(FileSaveRequest request, CancellationToken ct = default);

    /// <summary>Open a file for reading, or <c>null</c> if the key does not exist. Caller disposes the stream.</summary>
    Task<FileDownload?> OpenReadAsync(string storageKey, CancellationToken ct = default);

    /// <summary>Delete a file. Returns <c>false</c> if the key did not exist.</summary>
    Task<bool> DeleteAsync(string storageKey, CancellationToken ct = default);

    /// <summary>Whether a file exists for the given key.</summary>
    Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default);

    /// <summary>
    /// A time-limited direct URL for the file when the provider supports it (e.g. S3 presigned URL,
    /// or a configured public base URL for local). Returns <c>null</c> when the file must be streamed
    /// through the API instead.
    /// </summary>
    Task<string?> GetPresignedUrlAsync(string storageKey, TimeSpan expiry, CancellationToken ct = default);
}
