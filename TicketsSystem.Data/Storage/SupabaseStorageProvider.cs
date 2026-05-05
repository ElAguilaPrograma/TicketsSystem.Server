using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Data.Storage
{
    public class SupabaseStorageProvider : IFileStorageProvider
    {
        private readonly Supabase.Storage.Client _storage;

        public SupabaseStorageProvider(Supabase.Storage.Client storage)
        {
            _storage = storage;
        }

        public async Task<(string Url, string Path)> UploadAsync(string bucketName, string fileName, byte[] data, string contentType)
        {
            var bucket = _storage.From(bucketName);
            await bucket.Upload(data, fileName);

            var url = await bucket.CreateSignedUrl(fileName, 604800);

            return (url, fileName);
        }

        public async Task<(string Url, string Path)> UpdateAsync(string bucketName, string path, byte[] data, string contentType)
        {
            var bucket = _storage.From(bucketName);

            var options = new Supabase.Storage.FileOptions
            {
                Upsert = true,
                ContentType = contentType
            };

            await bucket.Update(data, path, options);

            var url = await bucket.CreateSignedUrl(path, 604800);

            return (url, path);
        }

        public async Task DeleteAsync(string bucketName, string path)
        {
            var bucket = _storage.From(bucketName);
            await bucket.Remove(path);
        }

        public async Task<bool> ExistsAsync(string bucketName, string path)
        {
            var bucket = _storage.From(bucketName);
            var info = await bucket.Info(path);
            return info != null;
        }
    }
}
