namespace EirMed.API.Models.Doctors;

public record DoctorResponse(
    Guid Id,
    string Nome,
    string Especialidade,
    string? RegistroProfissional,
    string? ClinicaHospital,
    string? Contato,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
