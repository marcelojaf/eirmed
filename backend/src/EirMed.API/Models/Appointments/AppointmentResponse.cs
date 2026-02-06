using EirMed.API.Models.Files;

namespace EirMed.API.Models.Appointments;

public record AppointmentResponse(
    Guid Id,
    DateTime Data,
    string Especialidade,
    string? QueixaPrincipal,
    string? ObservacoesGerais,
    string? Diagnosticos,
    Guid DoctorId,
    string DoctorNome,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<AttachmentResponse>? Attachments = null
);
