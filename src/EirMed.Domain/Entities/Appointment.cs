namespace EirMed.Domain.Entities;

public class Appointment : BaseEntity
{
    public DateTime Data { get; set; }
    public string Especialidade { get; set; } = string.Empty;
    public string? QueixaPrincipal { get; set; }
    public string? ObservacoesGerais { get; set; }
    public string? Diagnosticos { get; set; }

    // Foreign keys
    public Guid DoctorId { get; set; }

    // Navigation properties
    public Doctor Doctor { get; set; } = null!;
    public ICollection<Exam> Exams { get; set; } = [];
    public ICollection<Prescription> Prescriptions { get; set; } = [];
    public ICollection<AppointmentAttachment> Attachments { get; set; } = [];
}
