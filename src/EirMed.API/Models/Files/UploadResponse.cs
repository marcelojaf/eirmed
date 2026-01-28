namespace EirMed.API.Models.Files;

public record UploadResponse(
    Guid Id,
    string FileName,
    string FileUrl,
    string? ContentType,
    long FileSizeBytes,
    DateTime CreatedAt
);
