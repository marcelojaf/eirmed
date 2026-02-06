using System.ComponentModel.DataAnnotations;
using EirMed.Domain.Enums;

namespace EirMed.API.Models.Profile;

public record UpdateUserProfileRequest(
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 200 caracteres.")]
    string Nome,

    DateOnly? DataNascimento,

    BloodType? TipoSanguineo,

    [StringLength(1000, ErrorMessage = "As alergias devem ter no máximo 1000 caracteres.")]
    string? Alergias,

    [StringLength(2000, ErrorMessage = "As observações gerais devem ter no máximo 2000 caracteres.")]
    string? ObservacoesGerais
);
