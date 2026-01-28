namespace EirMed.Domain.Entities;

public class AppointmentAttachment : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long FileSizeBytes { get; set; }

    // Foreign keys
    public Guid AppointmentId { get; set; }

    // Navigation properties
    public Appointment Appointment { get; set; } = null!;
}
