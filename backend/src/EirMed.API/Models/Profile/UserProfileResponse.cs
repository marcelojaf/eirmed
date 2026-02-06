using EirMed.Domain.Enums;

namespace EirMed.API.Models.Profile;

public record UserProfileResponse(
    Guid Id,
    string Nome,
    string Email,
    string? ProfilePictureUrl,
    DateOnly? DataNascimento,
    BloodType? TipoSanguineo,
    string? Alergias,
    string? ObservacoesGerais,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
