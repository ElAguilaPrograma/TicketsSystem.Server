using FluentResults;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace TicketsSystem.Core.Interfaces
{
    public interface IStorageService
    {
        Task<Result> DeleteFile(string bucketName, string supabasePath);
        Task<Result<(string Url, string Path)>> UpdateFile(string bucketName, string supabasePath, IFormFile file);
        Task<Result<(string Url, string Path)>> Upload(string bucketName, IFormFile file);
    }
}
