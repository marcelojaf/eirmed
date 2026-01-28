namespace EirMed.Domain.Entities;

public class Doctor : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Especialidade { get; set; } = string.Empty;
    public string? RegistroProfissional { get; set; }
    public string? ClinicaHospital { get; set; }
    public string? Contato { get; set; }

    // Foreign keys
    public Guid UserId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = [];
}
