namespace SAV.Shared.Interfaces;

public interface IAuthenticationService
{
    Task<object?> LoginAsync(object request);
    Task<bool> RegisterAsync(object request, string roleName = "Client");
    Task<object?> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(int userId);
    Task<bool> CreateAdminAsync(string email, string password, string fullName);
}

public interface IJwtTokenService
{
    string GenerateAccessToken(object user, string roleName);
    string GenerateRefreshToken();
    object? GetPrincipalFromExpiredToken(string token);
}
