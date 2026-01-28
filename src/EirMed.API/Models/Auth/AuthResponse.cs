namespace EirMed.API.Models.Auth;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfo User
);

public record UserInfo(
    Guid Id,
    string Nome,
    string Email,
    string? ProfilePictureUrl
);
