namespace EirMed.API.Models.Medications;

public record MedicationResponse(
    Guid Id,
    string Nome,
    string? PrincipioAtivo,
    string Dosagem,
    string FormaFarmaceutica,
    DateOnly? DataValidade,
    int QuantidadeAtual,
    int QuantidadeMinima,
    string TipoUso,
    bool EstoqueBaixo,
    bool Vencido,
    bool ProximoVencimento,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
