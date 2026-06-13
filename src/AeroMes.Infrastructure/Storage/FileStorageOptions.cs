namespace AeroMes.Infrastructure.Storage;

/// <summary>
/// Binds the <c>Storage</c> configuration section. <see cref="Provider"/> selects the
/// active <see cref="AeroMes.Application.Storage.IFileStorage"/> implementation at startup:
/// <c>Local</c> (default, on-prem) or <c>S3</c> (MinIO / AWS).
/// </summary>
public sealed class FileStorageOptions
{
    public const string SectionName = "Storage";

    /// <summary><c>Local</c> | <c>S3</c>.</summary>
    public string Provider { get; set; } = "Local";

    /// <summary>Maximum accepted upload size in bytes. Default 50 MB.</summary>
    public long MaxSizeBytes { get; set; } = 50L * 1024 * 1024;

    /// <summary>Allowed MIME types. Empty array = allow any.</summary>
    public string[] AllowedContentTypes { get; set; } = [];

    public LocalStorageOptions Local { get; set; } = new();

    public S3StorageOptions S3 { get; set; } = new();
}

public sealed class LocalStorageOptions
{
    /// <summary>Root directory for stored files. Relative paths resolve against the content root.</summary>
    public string RootPath { get; set; } = "App_Data/files";

    /// <summary>
    /// Optional public base URL if files are served by a reverse proxy / CDN. When set,
    /// <c>GetPresignedUrlAsync</c> returns <c>{PublicBaseUrl}/{key}</c>; otherwise files stream via the API.
    /// </summary>
    public string? PublicBaseUrl { get; set; }
}

public sealed class S3StorageOptions
{
    /// <summary>S3 endpoint. For MinIO set e.g. <c>http://localhost:9000</c>; leave empty for AWS.</summary>
    public string? ServiceUrl { get; set; }

    public string Bucket { get; set; } = "aeromes";

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string Region { get; set; } = "us-east-1";

    /// <summary>Required <c>true</c> for MinIO and other path-style S3 servers.</summary>
    public bool ForcePathStyle { get; set; } = true;

    public int PresignExpiryMinutes { get; set; } = 60;
}
