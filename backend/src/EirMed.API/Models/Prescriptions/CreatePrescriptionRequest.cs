using System.ComponentModel.DataAnnotations;

namespace EirMed.API.Models.Prescriptions;

public record CreatePrescriptionRequest(
    [Required(ErrorMessage = "A data de início é obrigatória.")]
    DateOnly DataInicio,

    DateOnly? DataFim,

    [StringLength(500, ErrorMessage = "O motivo deve ter no máximo 500 caracteres.")]
    string? Motivo,

    [Required(ErrorMessage = "O medicamento é obrigatório.")]
    Guid MedicationId,

    [Required(ErrorMessage = "A consulta é obrigatória.")]
    Guid AppointmentId
);
