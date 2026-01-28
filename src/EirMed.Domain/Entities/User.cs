using EirMed.Domain.Enums;

namespace EirMed.Domain.Entities;

public class User : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public DateOnly DataNascimento { get; set; }
    public BloodType? TipoSanguineo { get; set; }
    public string? ObservacoesGerais { get; set; }

    // Navigation properties
    public ICollection<Doctor> Doctors { get; set; } = [];
    public ICollection<Medication> Medications { get; set; } = [];
}
