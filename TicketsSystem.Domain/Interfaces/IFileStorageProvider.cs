namespace TicketsSystem.Domain.Interfaces;

public interface IFileStorageProvider
{
    Task<string> UploadAsync(string bucketName, string fileName, byte[] data, string contentType);
    Task<string> UpdateAsync(string bucketName, string path, byte[] data, string contentType);
    Task<string> GetUrl(string bucketName, string path);
    Task DeleteAsync(string bucketName, string path);
    Task<bool> ExistsAsync(string bucketName, string path);
}
