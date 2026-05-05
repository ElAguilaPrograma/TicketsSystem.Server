using FluentResults;
using Microsoft.AspNetCore.Http;

namespace TicketsSystem.Core.Interfaces
{
    public interface IStorageService
    {
        Task<Result<(string Url, string Path)>> UploadAsync(string bucketName, IFormFile file);
        Task<Result<(string Url, string Path)>> UpdateFileAsync(string bucketName, string supabasePath, IFormFile file);
        Task<Result> DeleteFileAsync(string bucketName, string supabasePath);
    }
}
