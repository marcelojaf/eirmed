namespace EirMed.API.Models.Timeline;

/// <summary>
/// Representa um evento na linha do tempo médica.
/// Pode ser uma consulta, exame ou prescrição.
/// </summary>
public record TimelineEventResponse(
    Guid Id,
    DateTime Data,
    TimelineEventType Tipo,
    string Titulo,
    string? Subtitulo,
    string? Descricao,
    Guid? ReferenciaId,
    TimelineEventDetails? Detalhes
);

/// <summary>
/// Tipo do evento na timeline
/// </summary>
public enum TimelineEventType
{
    Consulta,
    Exame,
    Prescricao
}

/// <summary>
/// Detalhes adicionais específicos do tipo de evento
/// </summary>
public record TimelineEventDetails(
    // Para Consultas
    string? NomeMedico,
    string? Especialidade,
    string? Diagnosticos,

    // Para Exames
    string? TipoExame,
    string? Laboratorio,
    string? Resultados,

    // Para Prescrições
    string? NomeMedicamento,
    string? Dosagem,
    DateOnly? DataFim
);
