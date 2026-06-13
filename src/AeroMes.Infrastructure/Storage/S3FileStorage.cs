using AeroMes.Application.Storage;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AeroMes.Infrastructure.Storage;

/// <summary>
/// S3-compatible <see cref="IFileStorage"/> (AWS S3 or self-hosted MinIO). Opt-in via
/// <c>Storage:Provider = S3</c>. Supports presigned download URLs.
/// </summary>
public sealed class S3FileStorage : IFileStorage
{
    private readonly FileStorageOptions _options;
    private readonly S3StorageOptions _s3;
    private readonly IAmazonS3 _client;
    private readonly ILogger<S3FileStorage> _logger;
    private readonly SemaphoreSlim _bucketGate = new(1, 1);
    private bool _bucketReady;

    public S3FileStorage(IOptions<FileStorageOptions> options, ILogger<S3FileStorage> logger)
    {
        _options = options.Value;
        _s3 = _options.S3;
        _logger = logger;

        var config = new AmazonS3Config
        {
            ForcePathStyle = _s3.ForcePathStyle,
            AuthenticationRegion = _s3.Region,
        };
        if (!string.IsNullOrWhiteSpace(_s3.ServiceUrl))
            config.ServiceURL = _s3.ServiceUrl;

        var credentials = new BasicAWSCredentials(_s3.AccessKey, _s3.SecretKey);
        _client = new AmazonS3Client(credentials, config);
    }

    public async Task<StoredFileInfo> SaveAsync(FileSaveRequest request, CancellationToken ct = default)
    {
        FileStorageHelpers.Validate(request, _options);
        await EnsureBucketAsync(ct);

        var key = FileStorageHelpers.BuildKey(request.Container, request.FileName);
        var spooled = await FileStorageHelpers.SpoolToTempAsync(request.Content, _options.MaxSizeBytes, ct);

        try
        {
            await using var upload = new FileStream(
                spooled.TempPath, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 81920, useAsync: true);

            await _client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _s3.Bucket,
                Key = key,
                InputStream = upload,
                ContentType = request.ContentType,
                DisablePayloadSigning = !string.IsNullOrWhiteSpace(_s3.ServiceUrl), // MinIO friendliness
            }, ct);
        }
        finally
        {
            FileStorageHelpers.TryDelete(spooled.TempPath);
        }

        _logger.LogDebug("Stored file {Key} ({Size} bytes) in S3 bucket {Bucket}", key, spooled.SizeBytes, _s3.Bucket);
        return new StoredFileInfo(key, request.FileName, request.ContentType, spooled.SizeBytes, spooled.Checksum);
    }

    public async Task<FileDownload?> OpenReadAsync(string storageKey, CancellationToken ct = default)
    {
        try
        {
            var response = await _client.GetObjectAsync(_s3.Bucket, storageKey, ct);
            var fileName = Path.GetFileName(storageKey);
            return new FileDownload(
                response.ResponseStream,
                response.Headers.ContentType ?? "application/octet-stream",
                response.ContentLength,
                fileName);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<bool> DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        if (!await ExistsAsync(storageKey, ct))
            return false;

        await _client.DeleteObjectAsync(_s3.Bucket, storageKey, ct);
        return true;
    }

    public async Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default)
    {
        try
        {
            await _client.GetObjectMetadataAsync(_s3.Bucket, storageKey, ct);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task<string?> GetPresignedUrlAsync(string storageKey, TimeSpan expiry, CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _s3.Bucket,
            Key = storageKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiry),
        };
        return await _client.GetPreSignedURLAsync(request);
    }

    private async Task EnsureBucketAsync(CancellationToken ct)
    {
        if (_bucketReady) return;

        await _bucketGate.WaitAsync(ct);
        try
        {
            if (_bucketReady) return;

            if (!await AmazonS3Util.DoesS3BucketExistV2Async(_client, _s3.Bucket))
                await _client.PutBucketAsync(new PutBucketRequest { BucketName = _s3.Bucket }, ct);

            _bucketReady = true;
        }
        finally
        {
            _bucketGate.Release();
        }
    }
}
