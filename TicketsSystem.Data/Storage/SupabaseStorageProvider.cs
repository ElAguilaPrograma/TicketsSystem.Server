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

        public async Task<string> UploadAsync(string bucketName, string fileName, byte[] data, string contentType)
        {
            var bucket = _storage.From(bucketName);
            await bucket.Upload(data, fileName);

            return (fileName);
        }

        public async Task<string> GetUrl(string bucketName, string path)
        {
            var bucket = _storage.From(bucketName);
            var url = await bucket.CreateSignedUrl(path, 150); // URL válida por 5 minutos
            return url;
        }

        public async Task<string> UpdateAsync(string bucketName, string path, byte[] data, string contentType)
        {
            var bucket = _storage.From(bucketName);

            var options = new Supabase.Storage.FileOptions
            {
                Upsert = true,
                ContentType = contentType
            };

            await bucket.Update(data, path, options);

            return path;
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
