using EirMed.Domain.Enums;

namespace EirMed.API.Models.Exams;

public record ExamResponse(
    Guid Id,
    ExamType TipoExame,
    string TipoExameDescricao,
    string Nome,
    DateTime DataRealizacao,
    DateTime? DataResultado,
    string? Laboratorio,
    string? Resultados,
    string? FileUrl,
    Guid AppointmentId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
