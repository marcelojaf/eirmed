using System.Security.Claims;
using EirMed.API.Models.Auth;
using EirMed.Infrastructure.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EirMed.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return BadRequest(new { message = "Token do Google é obrigatório." });
        }

        var ipAddress = GetIpAddress();
        var result = await _authService.AuthenticateWithGoogleAsync(request.IdToken, ipAddress);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        var response = new AuthResponse(
            result.AccessToken!,
            result.RefreshToken!,
            result.ExpiresAt!.Value,
            new UserInfo(
                result.User!.Id,
                result.User.Nome,
                result.User.Email,
                result.User.ProfilePictureUrl
            )
        );

        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { message = "Refresh token é obrigatório." });
        }

        var ipAddress = GetIpAddress();
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

        if (!result.Success)
        {
            return Unauthorized(new { message = result.ErrorMessage });
        }

        var response = new AuthResponse(
            result.AccessToken!,
            result.RefreshToken!,
            result.ExpiresAt!.Value,
            new UserInfo(
                result.User!.Id,
                result.User.Nome,
                result.User.Email,
                result.User.ProfilePictureUrl
            )
        );

        return Ok(response);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { message = "Refresh token é obrigatório." });
        }

        var ipAddress = GetIpAddress();
        await _authService.RevokeTokenAsync(request.RefreshToken, ipAddress);

        return Ok(new { message = "Logout realizado com sucesso." });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        var user = await _authService.GetUserByIdAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "Usuário não encontrado." });
        }

        return Ok(new UserInfo(
            user.Id,
            user.Nome,
            user.Email,
            user.ProfilePictureUrl
        ));
    }

    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return Request.Headers["X-Forwarded-For"].ToString().Split(',').FirstOrDefault()?.Trim() ?? "unknown";
        }

        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";
    }
}
