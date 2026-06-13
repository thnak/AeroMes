using Amazon.S3;
using Amazon.S3.Model;
using AeroMes.Application.Storage;
using Microsoft.Extensions.Options;

namespace AeroMes.Infrastructure.Storage;

public sealed class S3StorageOptions
{
    public string ServiceUrl { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string BucketName { get; set; } = "aeromes";
    public bool ForcePathStyle { get; set; } = true;
}

public sealed class S3Storage(IAmazonS3 s3, IOptions<StorageOptions> opts) : IFileStorage
{
    private readonly string _bucket = opts.Value.S3.BucketName;

    public async Task<string> SaveAsync(Stream stream, string key, string contentType, CancellationToken ct)
    {
        var req = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
            AutoCloseStream = false,
        };
        await s3.PutObjectAsync(req, ct);
        return key;
    }

    public async Task<Stream> OpenReadAsync(string key, CancellationToken ct)
    {
        var resp = await s3.GetObjectAsync(_bucket, key, ct);
        return resp.ResponseStream;
    }

    public async Task DeleteAsync(string key, CancellationToken ct)
    {
        await s3.DeleteObjectAsync(_bucket, key, ct);
        // also delete thumbnail if present
        var thumbKey = $"thumbnails/{key}";
        try { await s3.DeleteObjectAsync(_bucket, thumbKey, ct); } catch { /* ok if missing */ }
    }

    public async Task<string> GetUrlAsync(string key, TimeSpan ttl, CancellationToken ct)
    {
        var req = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = key,
            Expires = DateTime.UtcNow.Add(ttl),
            Verb = HttpVerb.GET,
        };
        return s3.GetPreSignedURL(req);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct)
    {
        try
        {
            await s3.GetObjectMetadataAsync(_bucket, key, ct);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}
