using Amazon.S3;
using Amazon.S3.Model;
using Assura.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Assura.Infrastructure.Services;

/// <summary>
/// Stores uploads in a private S3 bucket instead of local disk, so files survive pod
/// reschedules/restarts under Kubernetes. Credentials come from the AWS SDK's default chain
/// (IRSA when running in EKS) — no static access keys are read from configuration here.
/// The bucket is private; reads go through a short-lived pre-signed URL rather than a public
/// object URL, matching the "deny all principals except this workload's IRSA role" bucket policy.
/// </summary>
public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;
    private readonly ILogger<S3FileStorageService> _logger;

    public S3FileStorageService(IAmazonS3 s3, IConfiguration configuration, ILogger<S3FileStorageService> logger)
    {
        _s3 = s3;
        _bucketName = configuration["Storage:S3:BucketName"]
            ?? throw new InvalidOperationException("Storage:S3:BucketName must be set when Storage:Provider is 'S3'.");
        _logger = logger;
    }

    public async Task<string> SaveAsync(Stream content, string subfolder, string fileName, string? contentType, CancellationToken cancellationToken = default)
    {
        var key = ToKey(subfolder, fileName);

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = content,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AWSKMS
        };

        await _s3.PutObjectAsync(request, cancellationToken);
        return $"/uploads/{key}";
    }

    public async Task DeleteAsync(string virtualPath, CancellationToken cancellationToken = default)
    {
        var key = FromVirtualPath(virtualPath);
        try
        {
            await _s3.DeleteObjectAsync(_bucketName, key, cancellationToken);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete S3 object {Key} from bucket {Bucket}", key, _bucketName);
        }
    }

    public async Task<string?> GetDownloadUrlAsync(string virtualPath, CancellationToken cancellationToken = default)
    {
        var key = FromVirtualPath(virtualPath);
        var url = await _s3.GetPreSignedURLAsync(new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(15),
            Verb = HttpVerb.GET
        });
        return url;
    }

    private static string ToKey(string subfolder, string fileName)
        => string.IsNullOrEmpty(subfolder) ? fileName : $"{subfolder}/{fileName}";

    private static string FromVirtualPath(string virtualPath)
        => virtualPath.TrimStart('/').Replace("uploads/", string.Empty, StringComparison.Ordinal);
}
