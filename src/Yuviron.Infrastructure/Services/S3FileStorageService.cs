using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Infrastructure.Services;

internal class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3FileStorageService(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS:BucketName"]
                      ?? throw new ArgumentNullException("AWS:BucketName не найден в конфиге");
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var uniqueKey = $"{Guid.NewGuid()}_{fileName}";

        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = uniqueKey,
            InputStream = stream,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(putRequest, cancellationToken);

        return $"https://{_bucketName}.s3.amazonaws.com/{uniqueKey}";
    }

    public async Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = fileKey
        };

        await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);
    }
}