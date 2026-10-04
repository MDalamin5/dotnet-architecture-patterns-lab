using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TEcommerceWebApi.Interfaces;

namespace TEcommerceWebApi.Services
{
    public class S3FileStorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _serviceUrl;
        private readonly ILogger<S3FileStorageService> _logger;

        // Allowed image extensions
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

        public S3FileStorageService(
            IAmazonS3 s3Client, 
            IConfiguration configuration, 
            ILogger<S3FileStorageService> logger)
        {
            _s3Client = s3Client;
            _logger = logger;

            var s3Settings = configuration.GetSection("S3Settings");
            _bucketName = s3Settings["BucketName"] ?? "byvstore-media";
            _serviceUrl = s3Settings["ServiceUrl"] ?? "http://localhost:9000";
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folderPath)
        {
            // 1. Validation: File must not be empty
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file was uploaded.");
            }

            // 2. Validation: File size
            if (file.Length > MaxFileSizeInBytes)
            {
                throw new ArgumentException("File size exceeds the 5MB limit.");
            }

            // 3. Validation: Extension whitelist
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"Invalid file type '{extension}'. Only .jpg, .jpeg, .png, and .webp are allowed.");
            }

            // 4. Ensure bucket exists and has public read access
            await EnsureBucketExistsAsync();

            // 5. Generate unique file key: e.g. "tenants/{tenantId}/products/{guid}.webp"
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var objectKey = string.IsNullOrWhiteSpace(folderPath) 
                ? uniqueFileName 
                : $"{folderPath.TrimEnd('/')}/{uniqueFileName}";

            // 6. Upload stream directly to MinIO / S3
            using var stream = file.OpenReadStream();
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                InputStream = stream,
                ContentType = file.ContentType
            };

            await _s3Client.PutObjectAsync(putRequest);
            _logger.LogInformation("📸 [MINIO UPLOAD] Uploaded file to: {Key}", objectKey);

            // 7. Return the public URL for browsers to render
            return $"{_serviceUrl}/{_bucketName}/{objectKey}";
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return;

            try
            {
                // Extract object key from URL
                var uri = new Uri(fileUrl);
                var objectKey = uri.AbsolutePath.TrimStart('/').Replace($"{_bucketName}/", "");

                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = objectKey
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);
                _logger.LogInformation("🗑️ [MINIO DELETE] Deleted file: {Key}", objectKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️ [MINIO ERROR] Failed to delete file at URL: {Url}", fileUrl);
            }
        }

        // Helper: Creates bucket on Day 1 and configures public read policy
        private async Task EnsureBucketExistsAsync()
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName);
            if (!bucketExists)
            {
                var putBucketRequest = new PutBucketRequest { BucketName = _bucketName };
                await _s3Client.PutBucketAsync(putBucketRequest);

                // Set public read policy so browser can load image URLs without auth tokens
                var publicReadPolicy = $@"{{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {{
                            ""Effect"": ""Allow"",
                            ""Principal"": ""*"",
                            ""Action"": [""s3:GetObject""],
                            ""Resource"": [""arn:aws:s3:::{_bucketName}/*""]
                        }}
                    ]
                }}";

                await _s3Client.PutBucketPolicyAsync(new PutBucketPolicyRequest
                {
                    BucketName = _bucketName,
                    Policy = publicReadPolicy
                });

                _logger.LogInformation("🪣 [MINIO INIT] Created bucket '{Bucket}' with public read access.", _bucketName);
            }
        }
    }
}