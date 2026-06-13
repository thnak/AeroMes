using System.Security.Cryptography;
using AeroMes.Application.Storage;

namespace AeroMes.Infrastructure.Storage;

/// <summary>
/// Shared validation, key generation and spooling logic used by every
/// <see cref="IFileStorage"/> provider so the rules stay identical across them.
/// </summary>
internal static class FileStorageHelpers
{
    /// <summary>Validate a request against the configured guards. Throws <see cref="ArgumentException"/> on violation.</summary>
    public static void Validate(FileSaveRequest request, FileStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Content);

        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new ArgumentException("FileName is required.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Container))
            throw new ArgumentException("Container is required.", nameof(request));

        if (options.AllowedContentTypes.Length > 0 &&
            !options.AllowedContentTypes.Contains(request.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Content type '{request.ContentType}' is not allowed.", nameof(request));
        }
    }

    /// <summary>
    /// Build an opaque, collision-free storage key: <c>{container}/{guid:N}{ext}</c>.
    /// The original file name is metadata kept by the caller, not part of the physical key.
    /// </summary>
    public static string BuildKey(string container, string fileName)
    {
        var segment = SanitizeSegment(container);
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return $"{segment}/{Guid.NewGuid():N}{ext}";
    }

    /// <summary>
    /// Stream <paramref name="source"/> to a temp file while computing its SHA-256 and size,
    /// enforcing <paramref name="maxBytes"/>. The returned temp path must be deleted by the caller.
    /// </summary>
    public static async Task<SpooledFile> SpoolToTempAsync(
        Stream source, long maxBytes, CancellationToken ct)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"aeromes-upload-{Guid.NewGuid():N}.tmp");
        long total = 0;
        using var sha = SHA256.Create();

        try
        {
            await using (var temp = new FileStream(
                tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                bufferSize: 81920, useAsync: true))
            {
                var buffer = new byte[81920];
                int read;
                while ((read = await source.ReadAsync(buffer, ct)) > 0)
                {
                    total += read;
                    if (total > maxBytes)
                        throw new ArgumentException($"File exceeds the maximum size of {maxBytes} bytes.");

                    sha.TransformBlock(buffer, 0, read, null, 0);
                    await temp.WriteAsync(buffer.AsMemory(0, read), ct);
                }
            }

            sha.TransformFinalBlock([], 0, 0);
            var checksum = Convert.ToHexStringLower(sha.Hash!);
            return new SpooledFile(tempPath, total, checksum);
        }
        catch
        {
            TryDelete(tempPath);
            throw;
        }
    }

    public static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch
        {
            // best-effort temp cleanup
        }
    }

    private static string SanitizeSegment(string value)
    {
        var trimmed = value.Trim().Trim('/').ToLowerInvariant();
        return string.IsNullOrEmpty(trimmed) ? StorageContainers.General : trimmed;
    }
}

/// <summary>A file spooled to a temp path, with its computed size and SHA-256 (lowercase hex).</summary>
internal sealed record SpooledFile(string TempPath, long SizeBytes, string Checksum);
