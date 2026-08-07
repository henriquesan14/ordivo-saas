using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Ordivo.Application.Abstractions.Storage;

namespace Ordivo.Infrastructure.Storage;

internal sealed class S3FileStorage(IAmazonS3 client, IOptions<S3Options> options) : IFileStorage
{
    private static readonly SemaphoreSlim BucketLock = new(1, 1);
    private static bool _bucketReady;
    private readonly S3Options _options = options.Value;

    public async Task UploadAsync(string key, Stream content, string contentType, CancellationToken ct)
    {
        await EnsureBucketAsync(ct);
        await client.PutObjectAsync(new PutObjectRequest { BucketName = _options.Bucket, Key = key, InputStream = content, ContentType = contentType, AutoCloseStream = false }, ct);
    }

    public async Task<StoredFile?> DownloadAsync(string key, CancellationToken ct)
    {
        await EnsureBucketAsync(ct);
        try
        {
            var response = await client.GetObjectAsync(_options.Bucket, key, ct);
            return new StoredFile(response.ResponseStream, response.Headers.ContentType ?? "application/octet-stream", response.ContentLength);
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound) { return null; }
    }

    public async Task DeleteAsync(string key, CancellationToken ct)
    {
        await EnsureBucketAsync(ct);
        await client.DeleteObjectAsync(_options.Bucket, key, ct);
    }

    private async Task EnsureBucketAsync(CancellationToken ct)
    {
        if (_bucketReady) return;
        await BucketLock.WaitAsync(ct);
        try
        {
            if (_bucketReady) return;
            var buckets = await client.ListBucketsAsync(ct);
            if (buckets.Buckets?.Any(bucket => bucket.BucketName == _options.Bucket) != true)
                await client.PutBucketAsync(new PutBucketRequest { BucketName = _options.Bucket }, ct);
            _bucketReady = true;
        }
        finally { BucketLock.Release(); }
    }
}
