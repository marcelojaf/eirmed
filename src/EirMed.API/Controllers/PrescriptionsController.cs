using System.Security.Claims;
using EirMed.API.Models.Prescriptions;
using EirMed.Domain.Entities;
using EirMed.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EirMed.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrescriptionsController : ControllerBase
{
    private readonly EirMedDbContext _context;

    public PrescriptionsController(EirMedDbContext context)
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

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var prescriptions = await _context.Prescriptions
            .AsNoTracking()
            .Include(p => p.Medication)
            .Include(p => p.Appointment)
                .ThenInclude(a => a.Doctor)
            .Where(p => p.Medication.UserId == userId)
            .OrderByDescending(p => p.DataInicio)
            .Select(p => new PrescriptionResponse(
                p.Id,
                p.DataInicio,
                p.DataFim,
                p.Motivo,
                p.MedicationId,
                p.Medication.Nome,
                p.Medication.Dosagem,
                p.AppointmentId,
                p.Appointment.Data,
                p.Appointment.Doctor.Nome,
                !p.DataFim.HasValue || p.DataFim.Value >= today,
                p.CreatedAt,
                p.UpdatedAt
            ))
            .ToListAsync();

        return Ok(prescriptions);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var prescription = await _context.Prescriptions
            .AsNoTracking()
            .Include(p => p.Medication)
            .Include(p => p.Appointment)
                .ThenInclude(a => a.Doctor)
            .Where(p => p.Id == id && p.Medication.UserId == userId)
            .Select(p => new PrescriptionResponse(
                p.Id,
                p.DataInicio,
                p.DataFim,
                p.Motivo,
                p.MedicationId,
                p.Medication.Nome,
                p.Medication.Dosagem,
                p.AppointmentId,
                p.Appointment.Data,
                p.Appointment.Doctor.Nome,
                !p.DataFim.HasValue || p.DataFim.Value >= today,
                p.CreatedAt,
                p.UpdatedAt
            ))
            .FirstOrDefaultAsync();

        if (prescription == null)
        {
            return NotFound(new { message = "Prescrição não encontrada." });
        }

        return Ok(prescription);
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

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var prescriptions = await _context.Prescriptions
            .AsNoTracking()
            .Include(p => p.Medication)
            .Include(p => p.Appointment)
                .ThenInclude(a => a.Doctor)
            .Where(p => p.AppointmentId == appointmentId)
            .OrderByDescending(p => p.DataInicio)
            .Select(p => new PrescriptionResponse(
                p.Id,
                p.DataInicio,
                p.DataFim,
                p.Motivo,
                p.MedicationId,
                p.Medication.Nome,
                p.Medication.Dosagem,
                p.AppointmentId,
                p.Appointment.Data,
                p.Appointment.Doctor.Nome,
                !p.DataFim.HasValue || p.DataFim.Value >= today,
                p.CreatedAt,
                p.UpdatedAt
            ))
            .ToListAsync();

        return Ok(prescriptions);
    }

    [HttpGet("by-medication/{medicationId:guid}")]
    public async Task<IActionResult> GetByMedication(Guid medicationId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        // Verify medication belongs to user
        var medicationExists = await _context.Medications
            .AnyAsync(m => m.Id == medicationId && m.UserId == userId);

        if (!medicationExists)
        {
            return NotFound(new { message = "Medicamento não encontrado." });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var prescriptions = await _context.Prescriptions
            .AsNoTracking()
            .Include(p => p.Medication)
            .Include(p => p.Appointment)
                .ThenInclude(a => a.Doctor)
            .Where(p => p.MedicationId == medicationId)
            .OrderByDescending(p => p.DataInicio)
            .Select(p => new PrescriptionResponse(
                p.Id,
                p.DataInicio,
                p.DataFim,
                p.Motivo,
                p.MedicationId,
                p.Medication.Nome,
                p.Medication.Dosagem,
                p.AppointmentId,
                p.Appointment.Data,
                p.Appointment.Doctor.Nome,
                !p.DataFim.HasValue || p.DataFim.Value >= today,
                p.CreatedAt,
                p.UpdatedAt
            ))
            .ToListAsync();

        return Ok(prescriptions);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var prescriptions = await _context.Prescriptions
            .AsNoTracking()
            .Include(p => p.Medication)
            .Include(p => p.Appointment)
                .ThenInclude(a => a.Doctor)
            .Where(p => p.Medication.UserId == userId)
            .Where(p => !p.DataFim.HasValue || p.DataFim.Value >= today)
            .OrderByDescending(p => p.DataInicio)
            .Select(p => new PrescriptionResponse(
                p.Id,
                p.DataInicio,
                p.DataFim,
                p.Motivo,
                p.MedicationId,
                p.Medication.Nome,
                p.Medication.Dosagem,
                p.AppointmentId,
                p.Appointment.Data,
                p.Appointment.Doctor.Nome,
                true,
                p.CreatedAt,
                p.UpdatedAt
            ))
            .ToListAsync();

        return Ok(prescriptions);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePrescriptionRequest request)
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

        // Verify medication belongs to user
        var medication = await _context.Medications
            .FirstOrDefaultAsync(m => m.Id == request.MedicationId && m.UserId == userId);

        if (medication == null)
        {
            return BadRequest(new { message = "Medicamento não encontrado ou não pertence ao usuário." });
        }

        // Verify appointment belongs to user
        var appointment = await _context.Appointments
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId && a.Doctor.UserId == userId);

        if (appointment == null)
        {
            return BadRequest(new { message = "Consulta não encontrada ou não pertence ao usuário." });
        }

        // Validate dates
        if (request.DataFim.HasValue && request.DataFim.Value < request.DataInicio)
        {
            return BadRequest(new { message = "A data de fim não pode ser anterior à data de início." });
        }

        var prescription = new Prescription
        {
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            Motivo = request.Motivo,
            MedicationId = request.MedicationId,
            AppointmentId = request.AppointmentId
        };

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var response = new PrescriptionResponse(
            prescription.Id,
            prescription.DataInicio,
            prescription.DataFim,
            prescription.Motivo,
            prescription.MedicationId,
            medication.Nome,
            medication.Dosagem,
            prescription.AppointmentId,
            appointment.Data,
            appointment.Doctor.Nome,
            !prescription.DataFim.HasValue || prescription.DataFim.Value >= today,
            prescription.CreatedAt,
            prescription.UpdatedAt
        );

        return CreatedAtAction(nameof(GetById), new { id = prescription.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePrescriptionRequest request)
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

        var prescription = await _context.Prescriptions
            .Include(p => p.Medication)
            .Include(p => p.Appointment)
                .ThenInclude(a => a.Doctor)
            .FirstOrDefaultAsync(p => p.Id == id && p.Medication.UserId == userId);

        if (prescription == null)
        {
            return NotFound(new { message = "Prescrição não encontrada." });
        }

        // If changing medication, verify the new one belongs to user
        if (request.MedicationId != prescription.MedicationId)
        {
            var newMedication = await _context.Medications
                .FirstOrDefaultAsync(m => m.Id == request.MedicationId && m.UserId == userId);

            if (newMedication == null)
            {
                return BadRequest(new { message = "Medicamento não encontrado ou não pertence ao usuário." });
            }
        }

        // Validate dates
        if (request.DataFim.HasValue && request.DataFim.Value < request.DataInicio)
        {
            return BadRequest(new { message = "A data de fim não pode ser anterior à data de início." });
        }

        prescription.DataInicio = request.DataInicio;
        prescription.DataFim = request.DataFim;
        prescription.Motivo = request.Motivo;
        prescription.MedicationId = request.MedicationId;
        prescription.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload medication if changed
        var medication = await _context.Medications
            .FirstOrDefaultAsync(m => m.Id == prescription.MedicationId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var response = new PrescriptionResponse(
            prescription.Id,
            prescription.DataInicio,
            prescription.DataFim,
            prescription.Motivo,
            prescription.MedicationId,
            medication!.Nome,
            medication.Dosagem,
            prescription.AppointmentId,
            prescription.Appointment.Data,
            prescription.Appointment.Doctor.Nome,
            !prescription.DataFim.HasValue || prescription.DataFim.Value >= today,
            prescription.CreatedAt,
            prescription.UpdatedAt
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

        var prescription = await _context.Prescriptions
            .Include(p => p.Medication)
            .FirstOrDefaultAsync(p => p.Id == id && p.Medication.UserId == userId);

        if (prescription == null)
        {
            return NotFound(new { message = "Prescrição não encontrada." });
        }

        _context.Prescriptions.Remove(prescription);
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
