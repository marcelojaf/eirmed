namespace EirMed.Infrastructure.Services.Storage;

public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file and returns the file URL
    /// </summary>
    /// <param name="stream">File content stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="contentType">MIME type of the file</param>
    /// <param name="userId">User ID for organizing files</param>
    /// <param name="category">Category for organizing files (e.g., "appointments", "exams")</param>
    /// <returns>Tuple with storage path and public URL</returns>
    Task<(string StoragePath, string FileUrl)> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        Guid userId,
        string category);

    /// <summary>
    /// Deletes a file by its storage path
    /// </summary>
    /// <param name="storagePath">The storage path returned from UploadAsync</param>
    Task DeleteAsync(string storagePath);

    /// <summary>
    /// Gets a file stream for download
    /// </summary>
    /// <param name="storagePath">The storage path</param>
    /// <returns>File stream and content type, or null if not found</returns>
    Task<(Stream? Stream, string? ContentType)> GetAsync(string storagePath);

    /// <summary>
    /// Checks if a file exists
    /// </summary>
    /// <param name="storagePath">The storage path</param>
    Task<bool> ExistsAsync(string storagePath);
}
