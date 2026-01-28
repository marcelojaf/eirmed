using System.Security.Claims;
using EirMed.API.Models.Doctors;
using EirMed.Domain.Entities;
using EirMed.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EirMed.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly EirMedDbContext _context;

    public DoctorsController(EirMedDbContext context)
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

        var doctors = await _context.Doctors
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderBy(d => d.Nome)
            .Select(d => new DoctorResponse(
                d.Id,
                d.Nome,
                d.Especialidade,
                d.RegistroProfissional,
                d.ClinicaHospital,
                d.Contato,
                d.CreatedAt,
                d.UpdatedAt
            ))
            .ToListAsync();

        return Ok(doctors);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var doctor = await _context.Doctors
            .AsNoTracking()
            .Where(d => d.Id == id && d.UserId == userId)
            .Select(d => new DoctorResponse(
                d.Id,
                d.Nome,
                d.Especialidade,
                d.RegistroProfissional,
                d.ClinicaHospital,
                d.Contato,
                d.CreatedAt,
                d.UpdatedAt
            ))
            .FirstOrDefaultAsync();

        if (doctor == null)
        {
            return NotFound(new { message = "Profissional de saúde não encontrado." });
        }

        return Ok(doctor);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDoctorRequest request)
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

        var doctor = new Doctor
        {
            Nome = request.Nome,
            Especialidade = request.Especialidade,
            RegistroProfissional = request.RegistroProfissional,
            ClinicaHospital = request.ClinicaHospital,
            Contato = request.Contato,
            UserId = userId.Value
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        var response = new DoctorResponse(
            doctor.Id,
            doctor.Nome,
            doctor.Especialidade,
            doctor.RegistroProfissional,
            doctor.ClinicaHospital,
            doctor.Contato,
            doctor.CreatedAt,
            doctor.UpdatedAt
        );

        return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDoctorRequest request)
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

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

        if (doctor == null)
        {
            return NotFound(new { message = "Profissional de saúde não encontrado." });
        }

        doctor.Nome = request.Nome;
        doctor.Especialidade = request.Especialidade;
        doctor.RegistroProfissional = request.RegistroProfissional;
        doctor.ClinicaHospital = request.ClinicaHospital;
        doctor.Contato = request.Contato;
        doctor.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var response = new DoctorResponse(
            doctor.Id,
            doctor.Nome,
            doctor.Especialidade,
            doctor.RegistroProfissional,
            doctor.ClinicaHospital,
            doctor.Contato,
            doctor.CreatedAt,
            doctor.UpdatedAt
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

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

        if (doctor == null)
        {
            return NotFound(new { message = "Profissional de saúde não encontrado." });
        }

        // Check if doctor has appointments
        var hasAppointments = await _context.Appointments
            .AnyAsync(a => a.DoctorId == id);

        if (hasAppointments)
        {
            return Conflict(new { message = "Não é possível excluir este profissional pois existem consultas vinculadas." });
        }

        _context.Doctors.Remove(doctor);
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
