namespace EirMed.API.Models.Files;

public record AttachmentResponse(
    Guid Id,
    string FileName,
    string FileUrl,
    string? ContentType,
    long FileSizeBytes,
    DateTime CreatedAt
);
