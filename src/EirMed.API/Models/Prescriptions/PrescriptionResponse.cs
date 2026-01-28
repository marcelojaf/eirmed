namespace EirMed.API.Models.Prescriptions;

public record PrescriptionResponse(
    Guid Id,
    DateOnly DataInicio,
    DateOnly? DataFim,
    string? Motivo,
    Guid MedicationId,
    string MedicationNome,
    string MedicationDosagem,
    Guid AppointmentId,
    DateTime AppointmentData,
    string DoctorNome,
    bool Ativa,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
