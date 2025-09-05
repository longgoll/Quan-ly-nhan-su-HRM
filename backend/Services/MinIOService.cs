using Minio;
using Minio.DataModel.Args;
using backend.Configurations;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace backend.Services
{
    public interface IMinIOService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null);
        Task<Stream> DownloadFileAsync(string filePath);
        Task<bool> DeleteFileAsync(string filePath);
        Task<string> GetFileUrlAsync(string filePath, int expiryInMinutes = 60);
        Task<bool> FileExistsAsync(string filePath);
        Task<List<string>> ListFilesAsync(string? prefix = null);
    }

    public class MinIOService : IMinIOService
    {
        private readonly IMinioClient _minioClient;
        private readonly MinIOSettings _settings;
        private readonly ILogger<MinIOService> _logger;

        public MinIOService(IMinioClient minioClient, IOptions<MinIOSettings> settings, ILogger<MinIOService> logger)
        {
            _minioClient = minioClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null)
        {
            try
            {
                // Ensure bucket exists
                await EnsureBucketExistsAsync();

                // Generate unique file name to prevent conflicts
                var uniqueFileName = GenerateUniqueFileName(fileName);
                var filePath = folder != null ? $"{folder}/{uniqueFileName}" : uniqueFileName;

                // Upload file
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(filePath)
                    .WithStreamData(fileStream)
                    .WithObjectSize(fileStream.Length)
                    .WithContentType(contentType);

                await _minioClient.PutObjectAsync(putObjectArgs);

                _logger.LogInformation("File uploaded successfully: {FilePath}", filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
                throw new InvalidOperationException($"Failed to upload file: {ex.Message}", ex);
            }
        }

        public async Task<Stream> DownloadFileAsync(string filePath)
        {
            try
            {
                var memoryStream = new MemoryStream();
                
                var getObjectArgs = new GetObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(filePath)
                    .WithCallbackStream(stream => stream.CopyTo(memoryStream));

                await _minioClient.GetObjectAsync(getObjectArgs);
                
                memoryStream.Position = 0;
                _logger.LogInformation("File downloaded successfully: {FilePath}", filePath);
                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file: {FilePath}", filePath);
                throw new InvalidOperationException($"Failed to download file: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(filePath);

                await _minioClient.RemoveObjectAsync(removeObjectArgs);
                
                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                return false;
            }
        }

        public async Task<string> GetFileUrlAsync(string filePath, int expiryInMinutes = 60)
        {
            try
            {
                var presignedGetObjectArgs = new PresignedGetObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(filePath)
                    .WithExpiry(expiryInMinutes * 60);

                var url = await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
                
                _logger.LogInformation("Generated presigned URL for file: {FilePath}", filePath);
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating file URL: {FilePath}", filePath);
                throw new InvalidOperationException($"Failed to generate file URL: {ex.Message}", ex);
            }
        }

        public async Task<bool> FileExistsAsync(string filePath)
        {
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(filePath);

                await _minioClient.StatObjectAsync(statObjectArgs);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<string>> ListFilesAsync(string? prefix = null)
        {
            try
            {
                var files = new List<string>();
                
                var listObjectsArgs = new ListObjectsArgs()
                    .WithBucket(_settings.BucketName)
                    .WithPrefix(prefix)
                    .WithRecursive(true);

                await foreach (var item in _minioClient.ListObjectsEnumAsync(listObjectsArgs))
                {
                    if (!item.IsDir)
                    {
                        files.Add(item.Key);
                    }
                }

                _logger.LogInformation("Listed {Count} files with prefix: {Prefix}", files.Count, prefix);
                return files;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files with prefix: {Prefix}", prefix);
                throw new InvalidOperationException($"Failed to list files: {ex.Message}", ex);
            }
        }

        private async Task EnsureBucketExistsAsync()
        {
            try
            {
                var bucketExistsArgs = new BucketExistsArgs()
                    .WithBucket(_settings.BucketName);

                var bucketExists = await _minioClient.BucketExistsAsync(bucketExistsArgs);
                
                if (!bucketExists)
                {
                    var makeBucketArgs = new MakeBucketArgs()
                        .WithBucket(_settings.BucketName);
                    
                    if (!string.IsNullOrEmpty(_settings.Region))
                    {
                        makeBucketArgs = makeBucketArgs.WithLocation(_settings.Region);
                    }

                    await _minioClient.MakeBucketAsync(makeBucketArgs);
                    _logger.LogInformation("Bucket created: {BucketName}", _settings.BucketName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring bucket exists: {BucketName}", _settings.BucketName);
                throw new InvalidOperationException($"Failed to ensure bucket exists: {ex.Message}", ex);
            }
        }

        private static string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            // Create a hash from the original filename and timestamp
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{nameWithoutExtension}_{timestamp}"));
            var hashString = Convert.ToHexString(hash)[..8]; // Take first 8 characters
            
            return $"{nameWithoutExtension}_{timestamp}_{hashString}{extension}";
        }
    }
}
