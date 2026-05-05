using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Supabase.Storage.Exceptions;
using TicketsSystem.Core.Errors;
using TicketsSystem.Core.Helpers;
using TicketsSystem.Core.Interfaces;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Core.Services
{
    public class StorageService : IStorageService
    {
        private readonly IFileStorageProvider _storageProvider;
        private readonly ILogger<StorageService> _logger;

        public StorageService(IFileStorageProvider storageProvider, ILogger<StorageService> logger)
        {
            _storageProvider = storageProvider;
            _logger = logger;
        }

        public async Task<Result<(string Url, string Path)>> UploadAsync(string bucketName, IFormFile file)
        {
            try
            {
                _logger.LogDebug("Starting file upload to bucket '{BucketName}'", bucketName);

                if (!FilesValidatorHelper.IsValidSize(file))
                    return Result.Fail(new PayloadTooLargeError("The file is too large, 25MB limit."));

                if (!FilesValidatorHelper.IsValidImage(file))
                    return Result.Fail(new UnsupportedMediaTypeError("Invalid file format, only images are accepted."));

                var data = await ReadFileBytesAsync(file);
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";

                var (url, path) = await _storageProvider.UploadAsync(bucketName, fileName, data, file.ContentType);

                _logger.LogDebug("File uploaded successfully to '{Url}'", url);
                return Result.Ok((url, path)).WithSuccess(new OkSuccess("File uploaded successfully."));
            }
            catch (SupabaseStorageException ex)
            {
                _logger.LogError(ex, "Supabase storage exception during upload to bucket '{BucketName}'", bucketName);
                return Result.Fail(new InternalServerError("An error occurred while trying to upload the file."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during upload to bucket '{BucketName}'", bucketName);
                return Result.Fail(new InternalServerError("An error occurred while trying to upload the file."));
            }
        }

        public async Task<Result<(string Url, string Path)>> UpdateFileAsync(string bucketName, string supabasePath, IFormFile file)
        {
            try
            {
                _logger.LogDebug("Starting file update in bucket '{BucketName}' at path '{Path}'", bucketName, supabasePath);

                if (!FilesValidatorHelper.IsValidSize(file))
                    return Result.Fail(new PayloadTooLargeError("The file is too large, 25MB limit."));

                if (!FilesValidatorHelper.IsValidImage(file))
                    return Result.Fail(new UnsupportedMediaTypeError("Invalid file format, only images are accepted."));

                var exists = await _storageProvider.ExistsAsync(bucketName, supabasePath);
                if (!exists)
                {
                    _logger.LogWarning("File not found in bucket '{BucketName}' at path '{Path}'", bucketName, supabasePath);
                    return Result.Fail(new NotFoundError("The file was not found."));
                }

                var data = await ReadFileBytesAsync(file);
                var (url, path) = await _storageProvider.UpdateAsync(bucketName, supabasePath, data, file.ContentType);

                _logger.LogDebug("File updated successfully at '{Url}'", url);
                return Result.Ok((url, path)).WithSuccess(new OkSuccess("File updated successfully."));
            }
            catch (SupabaseStorageException ex)
            {
                _logger.LogError(ex, "Supabase storage exception during update in bucket '{BucketName}'", bucketName);
                return Result.Fail(new InternalServerError("An error occurred while trying to update the file."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during update in bucket '{BucketName}'", bucketName);
                return Result.Fail(new InternalServerError("An error occurred while trying to update the file."));
            }
        }

        public async Task<Result> DeleteFileAsync(string bucketName, string supabasePath)
        {
            try
            {
                _logger.LogDebug("Starting file deletion from bucket '{BucketName}' at path '{Path}'", bucketName, supabasePath);

                var exists = await _storageProvider.ExistsAsync(bucketName, supabasePath);
                if (!exists)
                {
                    _logger.LogWarning("File not found for deletion in bucket '{BucketName}' at path '{Path}'", bucketName, supabasePath);
                    return Result.Fail(new NotFoundError("The file was not found."));
                }

                await _storageProvider.DeleteAsync(bucketName, supabasePath);

                _logger.LogDebug("File deleted successfully from bucket '{BucketName}' at path '{Path}'", bucketName, supabasePath);
                return Result.Ok();
            }
            catch (SupabaseStorageException ex)
            {
                _logger.LogError(ex, "Supabase storage exception during deletion from bucket '{BucketName}'", bucketName);
                return Result.Fail(new InternalServerError("An error occurred while trying to delete the file."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during deletion from bucket '{BucketName}'", bucketName);
                return Result.Fail(new InternalServerError("An error occurred while trying to delete the file."));
            }
        }

        private static async Task<byte[]> ReadFileBytesAsync(IFormFile file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            return stream.ToArray();
        }
    }
}
