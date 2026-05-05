namespace TicketsSystem.Domain.Interfaces;

public interface IFileStorageProvider
{
    Task<(string Url, string Path)> UploadAsync(string bucketName, string fileName, byte[] data, string contentType);
    Task<(string Url, string Path)> UpdateAsync(string bucketName, string path, byte[] data, string contentType);
    Task DeleteAsync(string bucketName, string path);
    Task<bool> ExistsAsync(string bucketName, string path);
}
