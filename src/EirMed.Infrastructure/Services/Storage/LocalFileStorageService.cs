using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace EirMed.Infrastructure.Services.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;
    private readonly string _basePath;

    public LocalFileStorageService(IOptions<FileStorageSettings> settings, IWebHostEnvironment environment)
    {
        _settings = settings.Value;
        _basePath = Path.Combine(environment.ContentRootPath, _settings.BasePath);

        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<(string StoragePath, string FileUrl)> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        Guid userId,
        string category)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var sanitizedFileName = SanitizeFileName(Path.GetFileNameWithoutExtension(fileName));
        var uniqueFileName = $"{sanitizedFileName}_{Guid.NewGuid():N}{extension}";

        // Create directory structure: uploads/{userId}/{category}/
        var userDirectory = Path.Combine(_basePath, userId.ToString(), category);
        if (!Directory.Exists(userDirectory))
        {
            Directory.CreateDirectory(userDirectory);
        }

        var filePath = Path.Combine(userDirectory, uniqueFileName);
        var storagePath = Path.Combine(userId.ToString(), category, uniqueFileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fileStream);

        var fileUrl = $"{_settings.BaseUrl}/{userId}/{category}/{uniqueFileName}";

        return (storagePath, fileUrl);
    }

    public Task DeleteAsync(string storagePath)
    {
        var filePath = Path.Combine(_basePath, storagePath);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    public Task<(Stream? Stream, string? ContentType)> GetAsync(string storagePath)
    {
        var filePath = Path.Combine(_basePath, storagePath);

        if (!File.Exists(filePath))
        {
            return Task.FromResult<(Stream?, string?)>((null, null));
        }

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var contentType = GetContentType(Path.GetExtension(filePath));

        return Task.FromResult<(Stream?, string?)>((stream, contentType));
    }

    public Task<bool> ExistsAsync(string storagePath)
    {
        var filePath = Path.Combine(_basePath, storagePath);
        return Task.FromResult(File.Exists(filePath));
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Length > 50 ? sanitized[..50] : sanitized;
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
    }
}
