using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SAV.Domain.Entities;
using SAV.Shared.Constants;
using SAV.Shared.Interfaces;

namespace SAV.Infrastructure.Identity;

/// <summary>
/// Service for generating and validating JWT tokens.
/// Includes role-based claims for authorization.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    /// <summary>
    /// Generate an access JWT token with user claims and role information.
    /// </summary>
    /// <param name="user">User entity to generate token for.</param>
    /// <param name="roleName">User's role name (Admin, Technician, Client).</param>
    /// <returns>Signed JWT token as string.</returns>
    /// <exception cref="ArgumentException">Thrown if user is not of type User.</exception>
    public string GenerateAccessToken(object user, string roleName)
    {
        if (user is not User userData)
            throw new ArgumentException("User must be of type User");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Build comprehensive claims list
        var claims = new List<Claim>
        {
            // Standard claims
            new Claim(ClaimTypes.NameIdentifier, userData.Id.ToString(), ClaimValueTypes.String),
            new Claim(ClaimTypes.Email, userData.Email ?? string.Empty, ClaimValueTypes.Email),
            new Claim(ClaimTypes.Name, userData.FullName, ClaimValueTypes.String),

            // Role claim for authorization
            new Claim(ClaimTypes.Role, roleName, ClaimValueTypes.String),

            // Custom claims
            new Claim(CustomClaimTypes.UserId, userData.Id.ToString(), ClaimValueTypes.String),
            new Claim(CustomClaimTypes.FullName, userData.FullName, ClaimValueTypes.String),
            new Claim(CustomClaimTypes.IsActive, userData.IsActive.ToString(), ClaimValueTypes.Boolean),

            // Token issued at
            new Claim(JwtClaimTypes.IssuedAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generate a secure refresh token using cryptographic random number generation.
    /// </summary>
    /// <returns>Refresh token as base64 string.</returns>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    /// <summary>
    /// Extract and validate claims from an expired token.
    /// Used for refresh token flow.
    /// </summary>
    /// <param name="token">Expired JWT token.</param>
    /// <returns>ClaimsPrincipal containing token claims.</returns>
    /// <exception cref="SecurityTokenException">Thrown if token is invalid.</exception>
    public object? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
            ValidateLifetime = false // Don't validate lifetime for expired tokens
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}

/// <summary>
/// Standard JWT claim type names.
/// </summary>
public static class JwtClaimTypes
{
    public const string IssuedAt = "iat";
    public const string NotBefore = "nbf";
    public const string Expiration = "exp";
}
