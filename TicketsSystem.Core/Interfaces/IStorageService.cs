using FluentResults;
using Microsoft.AspNetCore.Http;

namespace TicketsSystem.Core.Interfaces
{
    public interface IStorageService
    {
        Task<Result<(string Path, string fileName)>> UploadAsync(string bucketName, IFormFile file);
        Task<Result<string>> UpdateFileAsync(string bucketName, string supabasePath, IFormFile file);
        Task<Result> DeleteFileAsync(string bucketName, string supabasePath);
        Task<Result<string>> GetUrlAsync(string bucketName, string path);
    }
}
