namespace EirMed.Domain.Entities;

public class Prescription : BaseEntity
{
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public string? Motivo { get; set; }

    // Foreign keys
    public Guid MedicationId { get; set; }
    public Guid AppointmentId { get; set; }

    // Navigation properties
    public Medication Medication { get; set; } = null!;
    public Appointment Appointment { get; set; } = null!;
}
