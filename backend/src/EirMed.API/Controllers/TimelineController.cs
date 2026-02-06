using System.Security.Claims;
using EirMed.API.Models.Timeline;
using EirMed.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EirMed.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimelineController : ControllerBase
{
    private readonly EirMedDbContext _context;

    public TimelineController(EirMedDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém a linha do tempo médica unificada com consultas, exames e prescrições.
    /// Os eventos são ordenados do mais recente para o mais antigo.
    /// </summary>
    /// <param name="dataInicio">Data inicial para filtrar (opcional)</param>
    /// <param name="dataFim">Data final para filtrar (opcional)</param>
    /// <param name="tipo">Tipo de evento para filtrar: Consulta, Exame, Prescricao (opcional)</param>
    /// <returns>Lista de eventos da timeline ordenados por data decrescente</returns>
    [HttpGet]
    public async Task<IActionResult> GetTimeline(
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        [FromQuery] TimelineEventType? tipo)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var events = new List<TimelineEventResponse>();

        // Buscar consultas (se não há filtro de tipo ou se o tipo é Consulta)
        if (!tipo.HasValue || tipo == TimelineEventType.Consulta)
        {
            var appointmentsQuery = _context.Appointments
                .AsNoTracking()
                .Include(a => a.Doctor)
                .Where(a => a.Doctor.UserId == userId);

            if (dataInicio.HasValue)
                appointmentsQuery = appointmentsQuery.Where(a => a.Data >= dataInicio.Value);

            if (dataFim.HasValue)
                appointmentsQuery = appointmentsQuery.Where(a => a.Data <= dataFim.Value);

            var appointments = await appointmentsQuery.ToListAsync();

            events.AddRange(appointments.Select(a => new TimelineEventResponse(
                a.Id,
                a.Data,
                TimelineEventType.Consulta,
                $"Consulta - {a.Especialidade}",
                a.Doctor.Nome,
                a.QueixaPrincipal,
                a.Id,
                new TimelineEventDetails(
                    NomeMedico: a.Doctor.Nome,
                    Especialidade: a.Especialidade,
                    Diagnosticos: a.Diagnosticos,
                    TipoExame: null,
                    Laboratorio: null,
                    Resultados: null,
                    NomeMedicamento: null,
                    Dosagem: null,
                    DataFim: null
                )
            )));
        }

        // Buscar exames (se não há filtro de tipo ou se o tipo é Exame)
        if (!tipo.HasValue || tipo == TimelineEventType.Exame)
        {
            var examsQuery = _context.Exams
                .AsNoTracking()
                .Include(e => e.Appointment)
                    .ThenInclude(a => a.Doctor)
                .Where(e => e.Appointment.Doctor.UserId == userId);

            if (dataInicio.HasValue)
                examsQuery = examsQuery.Where(e => e.DataRealizacao >= dataInicio.Value);

            if (dataFim.HasValue)
                examsQuery = examsQuery.Where(e => e.DataRealizacao <= dataFim.Value);

            var exams = await examsQuery.ToListAsync();

            events.AddRange(exams.Select(e => new TimelineEventResponse(
                e.Id,
                e.DataRealizacao,
                TimelineEventType.Exame,
                $"Exame - {e.Nome}",
                e.TipoExame.ToString(),
                e.Laboratorio,
                e.Id,
                new TimelineEventDetails(
                    NomeMedico: e.Appointment.Doctor.Nome,
                    Especialidade: null,
                    Diagnosticos: null,
                    TipoExame: e.TipoExame.ToString(),
                    Laboratorio: e.Laboratorio,
                    Resultados: e.Resultados,
                    NomeMedicamento: null,
                    Dosagem: null,
                    DataFim: null
                )
            )));
        }

        // Buscar prescrições (se não há filtro de tipo ou se o tipo é Prescricao)
        if (!tipo.HasValue || tipo == TimelineEventType.Prescricao)
        {
            var prescriptionsQuery = _context.Prescriptions
                .AsNoTracking()
                .Include(p => p.Medication)
                .Include(p => p.Appointment)
                    .ThenInclude(a => a.Doctor)
                .Where(p => p.Appointment.Doctor.UserId == userId);

            if (dataInicio.HasValue)
            {
                var dataInicioOnly = DateOnly.FromDateTime(dataInicio.Value);
                prescriptionsQuery = prescriptionsQuery.Where(p => p.DataInicio >= dataInicioOnly);
            }

            if (dataFim.HasValue)
            {
                var dataFimOnly = DateOnly.FromDateTime(dataFim.Value);
                prescriptionsQuery = prescriptionsQuery.Where(p => p.DataInicio <= dataFimOnly);
            }

            var prescriptions = await prescriptionsQuery.ToListAsync();

            events.AddRange(prescriptions.Select(p => new TimelineEventResponse(
                p.Id,
                p.DataInicio.ToDateTime(TimeOnly.MinValue),
                TimelineEventType.Prescricao,
                $"Prescrição - {p.Medication.Nome}",
                p.Medication.Dosagem,
                p.Motivo,
                p.Id,
                new TimelineEventDetails(
                    NomeMedico: p.Appointment.Doctor.Nome,
                    Especialidade: null,
                    Diagnosticos: null,
                    TipoExame: null,
                    Laboratorio: null,
                    Resultados: null,
                    NomeMedicamento: p.Medication.Nome,
                    Dosagem: p.Medication.Dosagem,
                    DataFim: p.DataFim
                )
            )));
        }

        // Ordenar por data decrescente (mais recente primeiro)
        var sortedEvents = events.OrderByDescending(e => e.Data).ToList();

        return Ok(sortedEvents);
    }

    /// <summary>
    /// Obtém estatísticas resumidas da timeline do usuário.
    /// </summary>
    [HttpGet("estatisticas")]
    public async Task<IActionResult> GetEstatisticas()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var totalConsultas = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Doctor)
            .Where(a => a.Doctor.UserId == userId)
            .CountAsync();

        var totalExames = await _context.Exams
            .AsNoTracking()
            .Include(e => e.Appointment)
                .ThenInclude(a => a.Doctor)
            .Where(e => e.Appointment.Doctor.UserId == userId)
            .CountAsync();

        var totalPrescricoes = await _context.Prescriptions
            .AsNoTracking()
            .Include(p => p.Appointment)
                .ThenInclude(a => a.Doctor)
            .Where(p => p.Appointment.Doctor.UserId == userId)
            .CountAsync();

        var ultimaConsulta = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Doctor)
            .Where(a => a.Doctor.UserId == userId)
            .OrderByDescending(a => a.Data)
            .Select(a => (DateTime?)a.Data)
            .FirstOrDefaultAsync();

        var ultimoExame = await _context.Exams
            .AsNoTracking()
            .Include(e => e.Appointment)
                .ThenInclude(a => a.Doctor)
            .Where(e => e.Appointment.Doctor.UserId == userId)
            .OrderByDescending(e => e.DataRealizacao)
            .Select(e => (DateTime?)e.DataRealizacao)
            .FirstOrDefaultAsync();

        return Ok(new
        {
            totalConsultas,
            totalExames,
            totalPrescricoes,
            totalEventos = totalConsultas + totalExames + totalPrescricoes,
            ultimaConsulta,
            ultimoExame
        });
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}
