using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using EirMed.API.Models.Medications;
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
public class MedicationsController : ControllerBase
{
    private readonly EirMedDbContext _context;

    public MedicationsController(EirMedDbContext context)
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

        var medications = await _context.Medications
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.Nome)
            .Select(m => new MedicationResponse(
                m.Id,
                m.Nome,
                m.PrincipioAtivo,
                m.Dosagem,
                m.FormaFarmaceutica.ToString(),
                m.DataValidade,
                m.QuantidadeAtual,
                m.QuantidadeMinima,
                m.TipoUso.ToString(),
                m.QuantidadeAtual < m.QuantidadeMinima,
                m.DataValidade.HasValue && m.DataValidade.Value < DateOnly.FromDateTime(DateTime.UtcNow),
                m.DataValidade.HasValue && m.DataValidade.Value >= DateOnly.FromDateTime(DateTime.UtcNow) && m.DataValidade.Value <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
                m.CreatedAt,
                m.UpdatedAt
            ))
            .ToListAsync();

        return Ok(medications);
    }

    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thirtyDaysFromNow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

        var medications = await _context.Medications
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .Where(m =>
                m.QuantidadeAtual < m.QuantidadeMinima ||
                (m.DataValidade.HasValue && m.DataValidade.Value <= thirtyDaysFromNow))
            .OrderBy(m => m.DataValidade)
            .ThenBy(m => m.Nome)
            .Select(m => new MedicationResponse(
                m.Id,
                m.Nome,
                m.PrincipioAtivo,
                m.Dosagem,
                m.FormaFarmaceutica.ToString(),
                m.DataValidade,
                m.QuantidadeAtual,
                m.QuantidadeMinima,
                m.TipoUso.ToString(),
                m.QuantidadeAtual < m.QuantidadeMinima,
                m.DataValidade.HasValue && m.DataValidade.Value < today,
                m.DataValidade.HasValue && m.DataValidade.Value >= today && m.DataValidade.Value <= thirtyDaysFromNow,
                m.CreatedAt,
                m.UpdatedAt
            ))
            .ToListAsync();

        return Ok(medications);
    }

    [HttpGet("by-usage/{usageType}")]
    public async Task<IActionResult> GetByUsageType(string usageType)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        if (!Enum.TryParse<MedicationUsageType>(usageType, ignoreCase: true, out var parsedUsageType))
        {
            return BadRequest(new { message = "Tipo de uso inválido. Use: Continuous ou Occasional." });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thirtyDaysFromNow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

        var medications = await _context.Medications
            .AsNoTracking()
            .Where(m => m.UserId == userId && m.TipoUso == parsedUsageType)
            .OrderBy(m => m.Nome)
            .Select(m => new MedicationResponse(
                m.Id,
                m.Nome,
                m.PrincipioAtivo,
                m.Dosagem,
                m.FormaFarmaceutica.ToString(),
                m.DataValidade,
                m.QuantidadeAtual,
                m.QuantidadeMinima,
                m.TipoUso.ToString(),
                m.QuantidadeAtual < m.QuantidadeMinima,
                m.DataValidade.HasValue && m.DataValidade.Value < today,
                m.DataValidade.HasValue && m.DataValidade.Value >= today && m.DataValidade.Value <= thirtyDaysFromNow,
                m.CreatedAt,
                m.UpdatedAt
            ))
            .ToListAsync();

        return Ok(medications);
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
        var thirtyDaysFromNow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

        var medication = await _context.Medications
            .AsNoTracking()
            .Where(m => m.Id == id && m.UserId == userId)
            .Select(m => new MedicationResponse(
                m.Id,
                m.Nome,
                m.PrincipioAtivo,
                m.Dosagem,
                m.FormaFarmaceutica.ToString(),
                m.DataValidade,
                m.QuantidadeAtual,
                m.QuantidadeMinima,
                m.TipoUso.ToString(),
                m.QuantidadeAtual < m.QuantidadeMinima,
                m.DataValidade.HasValue && m.DataValidade.Value < today,
                m.DataValidade.HasValue && m.DataValidade.Value >= today && m.DataValidade.Value <= thirtyDaysFromNow,
                m.CreatedAt,
                m.UpdatedAt
            ))
            .FirstOrDefaultAsync();

        if (medication == null)
        {
            return NotFound(new { message = "Medicamento não encontrado." });
        }

        return Ok(medication);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMedicationRequest request)
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

        if (!Enum.TryParse<PharmaceuticalForm>(request.FormaFarmaceutica, ignoreCase: true, out var formaFarmaceutica))
        {
            return BadRequest(new { message = "Forma farmacêutica inválida." });
        }

        if (!Enum.TryParse<MedicationUsageType>(request.TipoUso, ignoreCase: true, out var tipoUso))
        {
            return BadRequest(new { message = "Tipo de uso inválido. Use: Continuous ou Occasional." });
        }

        var medication = new Medication
        {
            Nome = request.Nome,
            PrincipioAtivo = request.PrincipioAtivo,
            Dosagem = request.Dosagem,
            FormaFarmaceutica = formaFarmaceutica,
            DataValidade = request.DataValidade,
            QuantidadeAtual = request.QuantidadeAtual,
            QuantidadeMinima = request.QuantidadeMinima,
            TipoUso = tipoUso,
            UserId = userId.Value
        };

        _context.Medications.Add(medication);
        await _context.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thirtyDaysFromNow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

        var response = new MedicationResponse(
            medication.Id,
            medication.Nome,
            medication.PrincipioAtivo,
            medication.Dosagem,
            medication.FormaFarmaceutica.ToString(),
            medication.DataValidade,
            medication.QuantidadeAtual,
            medication.QuantidadeMinima,
            medication.TipoUso.ToString(),
            medication.QuantidadeAtual < medication.QuantidadeMinima,
            medication.DataValidade.HasValue && medication.DataValidade.Value < today,
            medication.DataValidade.HasValue && medication.DataValidade.Value >= today && medication.DataValidade.Value <= thirtyDaysFromNow,
            medication.CreatedAt,
            medication.UpdatedAt
        );

        return CreatedAtAction(nameof(GetById), new { id = medication.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMedicationRequest request)
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

        if (!Enum.TryParse<PharmaceuticalForm>(request.FormaFarmaceutica, ignoreCase: true, out var formaFarmaceutica))
        {
            return BadRequest(new { message = "Forma farmacêutica inválida." });
        }

        if (!Enum.TryParse<MedicationUsageType>(request.TipoUso, ignoreCase: true, out var tipoUso))
        {
            return BadRequest(new { message = "Tipo de uso inválido. Use: Continuous ou Occasional." });
        }

        var medication = await _context.Medications
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (medication == null)
        {
            return NotFound(new { message = "Medicamento não encontrado." });
        }

        medication.Nome = request.Nome;
        medication.PrincipioAtivo = request.PrincipioAtivo;
        medication.Dosagem = request.Dosagem;
        medication.FormaFarmaceutica = formaFarmaceutica;
        medication.DataValidade = request.DataValidade;
        medication.QuantidadeAtual = request.QuantidadeAtual;
        medication.QuantidadeMinima = request.QuantidadeMinima;
        medication.TipoUso = tipoUso;
        medication.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thirtyDaysFromNow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

        var response = new MedicationResponse(
            medication.Id,
            medication.Nome,
            medication.PrincipioAtivo,
            medication.Dosagem,
            medication.FormaFarmaceutica.ToString(),
            medication.DataValidade,
            medication.QuantidadeAtual,
            medication.QuantidadeMinima,
            medication.TipoUso.ToString(),
            medication.QuantidadeAtual < medication.QuantidadeMinima,
            medication.DataValidade.HasValue && medication.DataValidade.Value < today,
            medication.DataValidade.HasValue && medication.DataValidade.Value >= today && medication.DataValidade.Value <= thirtyDaysFromNow,
            medication.CreatedAt,
            medication.UpdatedAt
        );

        return Ok(response);
    }

    [HttpPatch("{id:guid}/stock")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var medication = await _context.Medications
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (medication == null)
        {
            return NotFound(new { message = "Medicamento não encontrado." });
        }

        medication.QuantidadeAtual = request.QuantidadeAtual;
        medication.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thirtyDaysFromNow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

        var response = new MedicationResponse(
            medication.Id,
            medication.Nome,
            medication.PrincipioAtivo,
            medication.Dosagem,
            medication.FormaFarmaceutica.ToString(),
            medication.DataValidade,
            medication.QuantidadeAtual,
            medication.QuantidadeMinima,
            medication.TipoUso.ToString(),
            medication.QuantidadeAtual < medication.QuantidadeMinima,
            medication.DataValidade.HasValue && medication.DataValidade.Value < today,
            medication.DataValidade.HasValue && medication.DataValidade.Value >= today && medication.DataValidade.Value <= thirtyDaysFromNow,
            medication.CreatedAt,
            medication.UpdatedAt
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

        var medication = await _context.Medications
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (medication == null)
        {
            return NotFound(new { message = "Medicamento não encontrado." });
        }

        // Check if medication has prescriptions
        var hasPrescriptions = await _context.Prescriptions
            .AnyAsync(p => p.MedicationId == id);

        if (hasPrescriptions)
        {
            return Conflict(new { message = "Não é possível excluir este medicamento pois existem prescrições vinculadas." });
        }

        _context.Medications.Remove(medication);
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

public record UpdateStockRequest(
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "A quantidade atual deve ser um número positivo.")]
    int QuantidadeAtual
);
