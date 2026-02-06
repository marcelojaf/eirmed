using EirMed.Domain.Enums;

namespace EirMed.Domain.Entities;

public class Medication : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string? PrincipioAtivo { get; set; }
    public string Dosagem { get; set; } = string.Empty;
    public PharmaceuticalForm FormaFarmaceutica { get; set; }
    public DateOnly? DataValidade { get; set; }
    public int QuantidadeAtual { get; set; }
    public int QuantidadeMinima { get; set; }
    public MedicationUsageType TipoUso { get; set; }

    // Foreign keys
    public Guid UserId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Prescription> Prescriptions { get; set; } = [];
}
