using EirMed.Domain.Entities;

namespace EirMed.Infrastructure.Services.Auth;

public interface IAuthService
{
    Task<AuthResult> AuthenticateWithGoogleAsync(string idToken, string ipAddress);
    Task<AuthResult> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task RevokeTokenAsync(string refreshToken, string ipAddress);
    Task<User?> GetUserByIdAsync(Guid userId);
}

public record AuthResult(
    bool Success,
    string? AccessToken = null,
    string? RefreshToken = null,
    DateTime? ExpiresAt = null,
    User? User = null,
    string? ErrorMessage = null
);
