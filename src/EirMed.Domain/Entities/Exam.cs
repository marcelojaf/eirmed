using EirMed.Domain.Enums;

namespace EirMed.Domain.Entities;

public class Exam : BaseEntity
{
    public ExamType TipoExame { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataRealizacao { get; set; }
    public DateTime? DataResultado { get; set; }
    public string? Laboratorio { get; set; }
    public string? Resultados { get; set; }
    public string? FileUrl { get; set; }

    // Foreign keys
    public Guid AppointmentId { get; set; }

    // Navigation properties
    public Appointment Appointment { get; set; } = null!;
}
