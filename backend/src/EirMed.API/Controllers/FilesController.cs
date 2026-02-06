using System.Security.Claims;
using EirMed.API.Models.Files;
using EirMed.Domain.Entities;
using EirMed.Infrastructure.Data;
using EirMed.Infrastructure.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EirMed.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly EirMedDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly FileStorageSettings _storageSettings;

    public FilesController(
        EirMedDbContext context,
        IFileStorageService fileStorage,
        IOptions<FileStorageSettings> storageSettings)
    {
        _context = context;
        _fileStorage = fileStorage;
        _storageSettings = storageSettings.Value;
    }

    /// <summary>
    /// Upload a file for an appointment
    /// </summary>
    [HttpPost("appointments/{appointmentId:guid}")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB
    public async Task<IActionResult> UploadForAppointment(Guid appointmentId, IFormFile file)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        // Validate appointment exists and belongs to user
        var appointment = await _context.Appointments
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.Doctor.UserId == userId);

        if (appointment == null)
        {
            return NotFound(new { message = "Consulta não encontrada." });
        }

        // Validate file
        var validationResult = ValidateFile(file);
        if (validationResult != null)
        {
            return validationResult;
        }

        // Upload file
        await using var stream = file.OpenReadStream();
        var (storagePath, fileUrl) = await _fileStorage.UploadAsync(
            stream,
            file.FileName,
            file.ContentType,
            userId.Value,
            "appointments");

        // Save attachment record
        var attachment = new AppointmentAttachment
        {
            FileName = file.FileName,
            FileUrl = fileUrl,
            StoragePath = storagePath,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            AppointmentId = appointmentId
        };

        _context.AppointmentAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        var response = new UploadResponse(
            attachment.Id,
            attachment.FileName,
            attachment.FileUrl,
            attachment.ContentType,
            attachment.FileSizeBytes,
            attachment.CreatedAt);

        return CreatedAtAction(nameof(GetAttachment), new { id = attachment.Id }, response);
    }

    /// <summary>
    /// Get attachment metadata
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAttachment(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var attachment = await _context.AppointmentAttachments
            .Include(a => a.Appointment)
            .ThenInclude(ap => ap.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id && a.Appointment.Doctor.UserId == userId);

        if (attachment == null)
        {
            return NotFound(new { message = "Arquivo não encontrado." });
        }

        var response = new AttachmentResponse(
            attachment.Id,
            attachment.FileName,
            attachment.FileUrl,
            attachment.ContentType,
            attachment.FileSizeBytes,
            attachment.CreatedAt);

        return Ok(response);
    }

    /// <summary>
    /// Download a file
    /// </summary>
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> DownloadFile(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var attachment = await _context.AppointmentAttachments
            .Include(a => a.Appointment)
            .ThenInclude(ap => ap.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id && a.Appointment.Doctor.UserId == userId);

        if (attachment == null)
        {
            return NotFound(new { message = "Arquivo não encontrado." });
        }

        var (stream, contentType) = await _fileStorage.GetAsync(attachment.StoragePath);
        if (stream == null)
        {
            return NotFound(new { message = "Arquivo não encontrado no storage." });
        }

        return File(stream, contentType ?? "application/octet-stream", attachment.FileName);
    }

    /// <summary>
    /// Get all attachments for an appointment
    /// </summary>
    [HttpGet("appointments/{appointmentId:guid}")]
    public async Task<IActionResult> GetAttachmentsByAppointment(Guid appointmentId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        // Validate appointment exists and belongs to user
        var appointmentExists = await _context.Appointments
            .Include(a => a.Doctor)
            .AnyAsync(a => a.Id == appointmentId && a.Doctor.UserId == userId);

        if (!appointmentExists)
        {
            return NotFound(new { message = "Consulta não encontrada." });
        }

        var attachments = await _context.AppointmentAttachments
            .Where(a => a.AppointmentId == appointmentId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AttachmentResponse(
                a.Id,
                a.FileName,
                a.FileUrl,
                a.ContentType,
                a.FileSizeBytes,
                a.CreatedAt))
            .ToListAsync();

        return Ok(attachments);
    }

    /// <summary>
    /// Delete an attachment
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAttachment(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var attachment = await _context.AppointmentAttachments
            .Include(a => a.Appointment)
            .ThenInclude(ap => ap.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id && a.Appointment.Doctor.UserId == userId);

        if (attachment == null)
        {
            return NotFound(new { message = "Arquivo não encontrado." });
        }

        // Delete from storage
        await _fileStorage.DeleteAsync(attachment.StoragePath);

        // Delete from database
        _context.AppointmentAttachments.Remove(attachment);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private IActionResult? ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Nenhum arquivo enviado." });
        }

        if (file.Length > _storageSettings.MaxFileSizeBytes)
        {
            var maxSizeMb = _storageSettings.MaxFileSizeBytes / (1024 * 1024);
            return BadRequest(new { message = $"O arquivo excede o tamanho máximo permitido de {maxSizeMb}MB." });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_storageSettings.AllowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = $"Tipo de arquivo não permitido. Extensões permitidas: {string.Join(", ", _storageSettings.AllowedExtensions)}" });
        }

        if (!string.IsNullOrEmpty(file.ContentType) && !_storageSettings.AllowedContentTypes.Contains(file.ContentType))
        {
            return BadRequest(new { message = "Tipo de conteúdo do arquivo não permitido." });
        }

        return null;
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
