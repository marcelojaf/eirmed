using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EirMed.Domain.Entities;
using EirMed.Infrastructure.Data;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EirMed.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    private readonly EirMedDbContext _context;
    private readonly JwtSettings _jwtSettings;
    private readonly GoogleSettings _googleSettings;

    public AuthService(
        EirMedDbContext context,
        IOptions<JwtSettings> jwtSettings,
        IOptions<GoogleSettings> googleSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
        _googleSettings = googleSettings.Value;
    }

    public async Task<AuthResult> AuthenticateWithGoogleAsync(string idToken, string ipAddress)
    {
        try
        {
            var payload = await ValidateGoogleTokenAsync(idToken);
            if (payload == null)
            {
                return new AuthResult(false, ErrorMessage: "Token do Google inválido.");
            }

            var user = await GetOrCreateUserAsync(payload);
            if (user == null || !user.IsActive)
            {
                return new AuthResult(false, ErrorMessage: "Usuário inativo ou não encontrado.");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var accessToken = GenerateAccessToken(user);
            var refreshToken = await GenerateRefreshTokenAsync(user.Id, ipAddress);

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

            return new AuthResult(
                true,
                accessToken,
                refreshToken.Token,
                expiresAt,
                user
            );
        }
        catch (Exception ex)
        {
            return new AuthResult(false, ErrorMessage: $"Erro na autenticação: {ex.Message}");
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var token = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token == null)
        {
            return new AuthResult(false, ErrorMessage: "Refresh token inválido.");
        }

        if (token.IsRevoked)
        {
            await RevokeDescendantTokensAsync(token, ipAddress, "Token pai revogado");
            return new AuthResult(false, ErrorMessage: "Refresh token foi revogado.");
        }

        if (token.IsExpired)
        {
            return new AuthResult(false, ErrorMessage: "Refresh token expirado.");
        }

        if (!token.User.IsActive)
        {
            return new AuthResult(false, ErrorMessage: "Usuário inativo.");
        }

        var newRefreshToken = await RotateRefreshTokenAsync(token, ipAddress);
        var accessToken = GenerateAccessToken(token.User);

        token.User.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        return new AuthResult(
            true,
            accessToken,
            newRefreshToken.Token,
            expiresAt,
            token.User
        );
    }

    public async Task RevokeTokenAsync(string refreshToken, string ipAddress)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token != null && token.IsActive)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = "Revogado pelo usuário";
            await _context.SaveChangesAsync();
        }
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    private async Task<GoogleJsonWebSignature.Payload?> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleSettings.ClientId }
            };

            return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch
        {
            return null;
        }
    }

    private async Task<User?> GetOrCreateUserAsync(GoogleJsonWebSignature.Payload payload)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.GoogleId == payload.Subject || u.Email == payload.Email);

        if (user == null)
        {
            user = new User
            {
                Email = payload.Email,
                Nome = payload.Name ?? payload.Email.Split('@')[0],
                GoogleId = payload.Subject,
                ProfilePictureUrl = payload.Picture,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        else
        {
            if (string.IsNullOrEmpty(user.GoogleId))
            {
                user.GoogleId = payload.Subject;
            }

            if (string.IsNullOrEmpty(user.ProfilePictureUrl) && !string.IsNullOrEmpty(payload.Picture))
            {
                user.ProfilePictureUrl = payload.Picture;
            }

            await _context.SaveChangesAsync();
        }

        return user;
    }

    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Nome),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("picture", user.ProfilePictureUrl ?? string.Empty)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string ipAddress)
    {
        var refreshToken = new RefreshToken
        {
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedByIp = ipAddress,
            UserId = userId
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }

    private async Task<RefreshToken> RotateRefreshTokenAsync(RefreshToken oldToken, string ipAddress)
    {
        var newRefreshToken = await GenerateRefreshTokenAsync(oldToken.UserId, ipAddress);

        oldToken.RevokedAt = DateTime.UtcNow;
        oldToken.RevokedByIp = ipAddress;
        oldToken.ReasonRevoked = "Substituído por novo token";
        oldToken.ReplacedByToken = newRefreshToken.Token;

        await _context.SaveChangesAsync();

        return newRefreshToken;
    }

    private async Task RevokeDescendantTokensAsync(RefreshToken token, string ipAddress, string reason)
    {
        if (string.IsNullOrEmpty(token.ReplacedByToken))
            return;

        var childToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token.ReplacedByToken);

        if (childToken != null)
        {
            if (childToken.IsActive)
            {
                childToken.RevokedAt = DateTime.UtcNow;
                childToken.RevokedByIp = ipAddress;
                childToken.ReasonRevoked = reason;
            }

            await RevokeDescendantTokensAsync(childToken, ipAddress, reason);
        }
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
