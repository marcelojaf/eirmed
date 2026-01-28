using System.Security.Claims;
using EirMed.API.Models.Profile;
using EirMed.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EirMed.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly EirMedDbContext _context;

    public UserProfileController(EirMedDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user == null)
        {
            return NotFound(new { message = "Usuário não encontrado." });
        }

        var response = new UserProfileResponse(
            user.Id,
            user.Nome,
            user.Email,
            user.ProfilePictureUrl,
            user.DataNascimento,
            user.TipoSanguineo,
            user.Alergias,
            user.ObservacoesGerais,
            user.CreatedAt,
            user.UpdatedAt
        );

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user == null)
        {
            return NotFound(new { message = "Usuário não encontrado." });
        }

        user.Nome = request.Nome;
        user.DataNascimento = request.DataNascimento;
        user.TipoSanguineo = request.TipoSanguineo;
        user.Alergias = request.Alergias;
        user.ObservacoesGerais = request.ObservacoesGerais;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var response = new UserProfileResponse(
            user.Id,
            user.Nome,
            user.Email,
            user.ProfilePictureUrl,
            user.DataNascimento,
            user.TipoSanguineo,
            user.Alergias,
            user.ObservacoesGerais,
            user.CreatedAt,
            user.UpdatedAt
        );

        return Ok(response);
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
