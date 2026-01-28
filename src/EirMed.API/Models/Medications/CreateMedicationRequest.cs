using System.ComponentModel.DataAnnotations;

namespace EirMed.API.Models.Medications;

public record CreateMedicationRequest(
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 200 caracteres.")]
    string Nome,

    [StringLength(200, ErrorMessage = "O princípio ativo deve ter no máximo 200 caracteres.")]
    string? PrincipioAtivo,

    [Required(ErrorMessage = "A dosagem é obrigatória.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "A dosagem deve ter entre 1 e 100 caracteres.")]
    string Dosagem,

    [Required(ErrorMessage = "A forma farmacêutica é obrigatória.")]
    string FormaFarmaceutica,

    DateOnly? DataValidade,

    [Required(ErrorMessage = "A quantidade atual é obrigatória.")]
    [Range(0, int.MaxValue, ErrorMessage = "A quantidade atual deve ser um número positivo.")]
    int QuantidadeAtual,

    [Required(ErrorMessage = "A quantidade mínima é obrigatória.")]
    [Range(0, int.MaxValue, ErrorMessage = "A quantidade mínima deve ser um número positivo.")]
    int QuantidadeMinima,

    [Required(ErrorMessage = "O tipo de uso é obrigatório.")]
    string TipoUso
);
