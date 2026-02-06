using EirMed.Domain.Enums;

namespace EirMed.Domain.Entities;

public class User : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? GoogleId { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateOnly? DataNascimento { get; set; }
    public BloodType? TipoSanguineo { get; set; }
    public string? Alergias { get; set; }
    public string? ObservacoesGerais { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Doctor> Doctors { get; set; } = [];
    public ICollection<Medication> Medications { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
