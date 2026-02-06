using System.Security.Claims;
using EirMed.API.Models.Exams;
using EirMed.Domain.Entities;
using EirMed.Domain.Enums;
using EirMed.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EirMed.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExamsController : ControllerBase
{
    private readonly EirMedDbContext _context;

    public ExamsController(EirMedDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var exams = await _context.Exams
            .AsNoTracking()
            .Include(e => e.Appointment)
                .ThenInclude(a => a.Doctor)
            .Where(e => e.Appointment.Doctor.UserId == userId)
            .OrderByDescending(e => e.DataRealizacao)
            .Select(e => new ExamResponse(
                e.Id,
                e.TipoExame,
                GetExamTypeDescription(e.TipoExame),
                e.Nome,
                e.DataRealizacao,
                e.DataResultado,
                e.Laboratorio,
                e.Resultados,
                e.FileUrl,
                e.AppointmentId,
                e.CreatedAt,
                e.UpdatedAt
            ))
            .ToListAsync();

        return Ok(exams);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var exam = await _context.Exams
            .AsNoTracking()
            .Include(e => e.Appointment)
                .ThenInclude(a => a.Doctor)
            .Where(e => e.Id == id && e.Appointment.Doctor.UserId == userId)
            .Select(e => new ExamResponse(
                e.Id,
                e.TipoExame,
                GetExamTypeDescription(e.TipoExame),
                e.Nome,
                e.DataRealizacao,
                e.DataResultado,
                e.Laboratorio,
                e.Resultados,
                e.FileUrl,
                e.AppointmentId,
                e.CreatedAt,
                e.UpdatedAt
            ))
            .FirstOrDefaultAsync();

        if (exam == null)
        {
            return NotFound(new { message = "Exame não encontrado." });
        }

        return Ok(exam);
    }

    [HttpGet("by-appointment/{appointmentId:guid}")]
    public async Task<IActionResult> GetByAppointment(Guid appointmentId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        // Verify appointment belongs to user
        var appointmentExists = await _context.Appointments
            .AnyAsync(a => a.Id == appointmentId && a.Doctor.UserId == userId);

        if (!appointmentExists)
        {
            return NotFound(new { message = "Consulta não encontrada." });
        }

        var exams = await _context.Exams
            .AsNoTracking()
            .Where(e => e.AppointmentId == appointmentId)
            .OrderByDescending(e => e.DataRealizacao)
            .Select(e => new ExamResponse(
                e.Id,
                e.TipoExame,
                GetExamTypeDescription(e.TipoExame),
                e.Nome,
                e.DataRealizacao,
                e.DataResultado,
                e.Laboratorio,
                e.Resultados,
                e.FileUrl,
                e.AppointmentId,
                e.CreatedAt,
                e.UpdatedAt
            ))
            .ToListAsync();

        return Ok(exams);
    }

    [HttpGet("by-type/{examType}")]
    public async Task<IActionResult> GetByType(ExamType examType)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var exams = await _context.Exams
            .AsNoTracking()
            .Include(e => e.Appointment)
                .ThenInclude(a => a.Doctor)
            .Where(e => e.TipoExame == examType && e.Appointment.Doctor.UserId == userId)
            .OrderByDescending(e => e.DataRealizacao)
            .Select(e => new ExamResponse(
                e.Id,
                e.TipoExame,
                GetExamTypeDescription(e.TipoExame),
                e.Nome,
                e.DataRealizacao,
                e.DataResultado,
                e.Laboratorio,
                e.Resultados,
                e.FileUrl,
                e.AppointmentId,
                e.CreatedAt,
                e.UpdatedAt
            ))
            .ToListAsync();

        return Ok(exams);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExamRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verify appointment belongs to user
        var appointment = await _context.Appointments
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId && a.Doctor.UserId == userId);

        if (appointment == null)
        {
            return NotFound(new { message = "Consulta não encontrada." });
        }

        var exam = new Exam
        {
            TipoExame = request.TipoExame,
            Nome = request.Nome,
            DataRealizacao = request.DataRealizacao,
            DataResultado = request.DataResultado,
            Laboratorio = request.Laboratorio,
            Resultados = request.Resultados,
            FileUrl = request.FileUrl,
            AppointmentId = request.AppointmentId
        };

        _context.Exams.Add(exam);
        await _context.SaveChangesAsync();

        var response = new ExamResponse(
            exam.Id,
            exam.TipoExame,
            GetExamTypeDescription(exam.TipoExame),
            exam.Nome,
            exam.DataRealizacao,
            exam.DataResultado,
            exam.Laboratorio,
            exam.Resultados,
            exam.FileUrl,
            exam.AppointmentId,
            exam.CreatedAt,
            exam.UpdatedAt
        );

        return CreatedAtAction(nameof(GetById), new { id = exam.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExamRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var exam = await _context.Exams
            .Include(e => e.Appointment)
                .ThenInclude(a => a.Doctor)
            .FirstOrDefaultAsync(e => e.Id == id && e.Appointment.Doctor.UserId == userId);

        if (exam == null)
        {
            return NotFound(new { message = "Exame não encontrado." });
        }

        exam.TipoExame = request.TipoExame;
        exam.Nome = request.Nome;
        exam.DataRealizacao = request.DataRealizacao;
        exam.DataResultado = request.DataResultado;
        exam.Laboratorio = request.Laboratorio;
        exam.Resultados = request.Resultados;
        exam.FileUrl = request.FileUrl;
        exam.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var response = new ExamResponse(
            exam.Id,
            exam.TipoExame,
            GetExamTypeDescription(exam.TipoExame),
            exam.Nome,
            exam.DataRealizacao,
            exam.DataResultado,
            exam.Laboratorio,
            exam.Resultados,
            exam.FileUrl,
            exam.AppointmentId,
            exam.CreatedAt,
            exam.UpdatedAt
        );

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var exam = await _context.Exams
            .Include(e => e.Appointment)
                .ThenInclude(a => a.Doctor)
            .FirstOrDefaultAsync(e => e.Id == id && e.Appointment.Doctor.UserId == userId);

        if (exam == null)
        {
            return NotFound(new { message = "Exame não encontrado." });
        }

        _context.Exams.Remove(exam);
        await _context.SaveChangesAsync();

        return NoContent();
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

    private static string GetExamTypeDescription(ExamType examType)
    {
        return examType switch
        {
            ExamType.Blood => "Sangue",
            ExamType.Image => "Imagem",
            ExamType.Functional => "Funcional",
            ExamType.Laboratory => "Laboratorial",
            ExamType.Other => "Outro",
            _ => examType.ToString()
        };
    }
}
