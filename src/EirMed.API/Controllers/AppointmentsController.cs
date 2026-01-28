using System.Security.Claims;
using EirMed.API.Models.Appointments;
using EirMed.API.Models.Files;
using EirMed.Domain.Entities;
using EirMed.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EirMed.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly EirMedDbContext _context;

    public AppointmentsController(EirMedDbContext context)
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

        var appointments = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Doctor)
            .Include(a => a.Attachments)
            .Where(a => a.Doctor.UserId == userId)
            .OrderByDescending(a => a.Data)
            .Select(a => new AppointmentResponse(
                a.Id,
                a.Data,
                a.Especialidade,
                a.QueixaPrincipal,
                a.ObservacoesGerais,
                a.Diagnosticos,
                a.DoctorId,
                a.Doctor.Nome,
                a.CreatedAt,
                a.UpdatedAt,
                a.Attachments.Select(att => new AttachmentResponse(
                    att.Id,
                    att.FileName,
                    att.FileUrl,
                    att.ContentType,
                    att.FileSizeBytes,
                    att.CreatedAt
                )).ToList()
            ))
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var appointment = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Doctor)
            .Include(a => a.Attachments)
            .Where(a => a.Id == id && a.Doctor.UserId == userId)
            .Select(a => new AppointmentResponse(
                a.Id,
                a.Data,
                a.Especialidade,
                a.QueixaPrincipal,
                a.ObservacoesGerais,
                a.Diagnosticos,
                a.DoctorId,
                a.Doctor.Nome,
                a.CreatedAt,
                a.UpdatedAt,
                a.Attachments.Select(att => new AttachmentResponse(
                    att.Id,
                    att.FileName,
                    att.FileUrl,
                    att.ContentType,
                    att.FileSizeBytes,
                    att.CreatedAt
                )).ToList()
            ))
            .FirstOrDefaultAsync();

        if (appointment == null)
        {
            return NotFound(new { message = "Consulta não encontrada." });
        }

        return Ok(appointment);
    }

    [HttpGet("by-doctor/{doctorId:guid}")]
    public async Task<IActionResult> GetByDoctor(Guid doctorId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        // Verify the doctor belongs to the current user
        var doctorBelongsToUser = await _context.Doctors
            .AnyAsync(d => d.Id == doctorId && d.UserId == userId);

        if (!doctorBelongsToUser)
        {
            return NotFound(new { message = "Profissional de saúde não encontrado." });
        }

        var appointments = await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Doctor)
            .Include(a => a.Attachments)
            .Where(a => a.DoctorId == doctorId)
            .OrderByDescending(a => a.Data)
            .Select(a => new AppointmentResponse(
                a.Id,
                a.Data,
                a.Especialidade,
                a.QueixaPrincipal,
                a.ObservacoesGerais,
                a.Diagnosticos,
                a.DoctorId,
                a.Doctor.Nome,
                a.CreatedAt,
                a.UpdatedAt,
                a.Attachments.Select(att => new AttachmentResponse(
                    att.Id,
                    att.FileName,
                    att.FileUrl,
                    att.ContentType,
                    att.FileSizeBytes,
                    att.CreatedAt
                )).ToList()
            ))
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
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

        // Verify the doctor belongs to the current user
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId && d.UserId == userId);

        if (doctor == null)
        {
            return BadRequest(new { message = "Profissional de saúde não encontrado ou não pertence ao usuário." });
        }

        var appointment = new Appointment
        {
            Data = request.Data,
            Especialidade = request.Especialidade,
            QueixaPrincipal = request.QueixaPrincipal,
            ObservacoesGerais = request.ObservacoesGerais,
            Diagnosticos = request.Diagnosticos,
            DoctorId = request.DoctorId
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        var response = new AppointmentResponse(
            appointment.Id,
            appointment.Data,
            appointment.Especialidade,
            appointment.QueixaPrincipal,
            appointment.ObservacoesGerais,
            appointment.Diagnosticos,
            appointment.DoctorId,
            doctor.Nome,
            appointment.CreatedAt,
            appointment.UpdatedAt,
            [] // New appointment has no attachments yet
        );

        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAppointmentRequest request)
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

        var appointment = await _context.Appointments
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id && a.Doctor.UserId == userId);

        if (appointment == null)
        {
            return NotFound(new { message = "Consulta não encontrada." });
        }

        // If changing doctor, verify the new doctor belongs to the user
        if (request.DoctorId != appointment.DoctorId)
        {
            var newDoctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == request.DoctorId && d.UserId == userId);

            if (newDoctor == null)
            {
                return BadRequest(new { message = "Profissional de saúde não encontrado ou não pertence ao usuário." });
            }
        }

        appointment.Data = request.Data;
        appointment.Especialidade = request.Especialidade;
        appointment.QueixaPrincipal = request.QueixaPrincipal;
        appointment.ObservacoesGerais = request.ObservacoesGerais;
        appointment.Diagnosticos = request.Diagnosticos;
        appointment.DoctorId = request.DoctorId;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload doctor name and attachments
        var doctorName = await _context.Doctors
            .Where(d => d.Id == appointment.DoctorId)
            .Select(d => d.Nome)
            .FirstOrDefaultAsync();

        var attachments = await _context.AppointmentAttachments
            .Where(a => a.AppointmentId == appointment.Id)
            .Select(att => new AttachmentResponse(
                att.Id,
                att.FileName,
                att.FileUrl,
                att.ContentType,
                att.FileSizeBytes,
                att.CreatedAt
            ))
            .ToListAsync();

        var response = new AppointmentResponse(
            appointment.Id,
            appointment.Data,
            appointment.Especialidade,
            appointment.QueixaPrincipal,
            appointment.ObservacoesGerais,
            appointment.Diagnosticos,
            appointment.DoctorId,
            doctorName ?? string.Empty,
            appointment.CreatedAt,
            appointment.UpdatedAt,
            attachments
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

        var appointment = await _context.Appointments
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id && a.Doctor.UserId == userId);

        if (appointment == null)
        {
            return NotFound(new { message = "Consulta não encontrada." });
        }

        // Check for related exams
        var hasExams = await _context.Exams
            .AnyAsync(e => e.AppointmentId == id);

        if (hasExams)
        {
            return Conflict(new { message = "Não é possível excluir esta consulta pois existem exames vinculados." });
        }

        // Check for related prescriptions
        var hasPrescriptions = await _context.Prescriptions
            .AnyAsync(p => p.AppointmentId == id);

        if (hasPrescriptions)
        {
            return Conflict(new { message = "Não é possível excluir esta consulta pois existem prescrições vinculadas." });
        }

        _context.Appointments.Remove(appointment);
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
}
