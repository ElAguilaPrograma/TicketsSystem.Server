using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Supabase.Storage;
using TicketsSystem.Core.Errors;
using TicketsSystem.Core.Interfaces;

namespace TicketsSystem.Core.Services
{
    public class SupabaseStorageService : IStorageService
    {
        private readonly Client _storage;

        public SupabaseStorageService(Client storage, IConfiguration configuration)
        {
            _storage = storage;
        }

        public async Task<Result<(string Url, string Path)>> Upload(string bucketName, IFormFile file)
        {
            Console.WriteLine("Empezando a subir archivo.....");

            var bucket = _storage.From(bucketName);
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";

            var result = await bucket.Upload(stream.ToArray(), fileName);

            if (result == null)
                return Result.Fail(new InternalServerError("An error occurred while trying to upload the file."));

            var url = bucket.GetPublicUrl(fileName);

            return Result.Ok((Url: url, Path: fileName)).WithSuccess(new OkSuccess("File uploaded successfully."));
        }

        public async Task<Result<(string Url, string Path)>> UpdateFile(string bucketName, string supabasePath, IFormFile file)
        {
            Console.WriteLine("Empezando a actualizar archivo.....");

            var bucket = _storage.From(bucketName);
            var info = await bucket.Info(supabasePath);

            if (info == null)
                return Result.Fail(new NotFoundError("The file was not found."));

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            var result = await bucket.Update(stream.ToArray(), supabasePath);

            if (result == null)
                return Result.Fail(new InternalServerError("An error occurred while trying to update the file."));

            var url = bucket.GetPublicUrl(supabasePath);

            return Result.Ok((Url: url, Path: supabasePath)).WithSuccess(new OkSuccess("File uploaded successfully."));
        }

        public async Task<Result> DeleteFile(string bucketName, string supabasePath)
        {
            var bucket = _storage.From(bucketName);
            var info = await bucket.Info(supabasePath);
            if (info == null)
                return Result.Fail(new NotFoundError("The file was not found."));

            var result = await bucket.Remove(supabasePath);

            if (result == null)
                return Result.Fail(new InternalServerError("An error occurred while trying to delete the file."));

            return Result.Ok();
        }

    }
}
