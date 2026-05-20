using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SAV.Shared.Constants;
using SAV.Shared.Interfaces;

namespace SAV.Infrastructure.Identity;

/// <summary>
/// Service for retrieving information about the current authenticated user.
/// Uses ClaimsPrincipal from HttpContext to extract user claims.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Get the current user's ID from claims.
    /// Returns null if user is not authenticated.
    /// </summary>
    public int? GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userId, out var id) ? id : null;
    }

    /// <summary>
    /// Get the current user's email from claims.
    /// </summary>
    public string? GetCurrentUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Get the current user's full name from claims.
    /// </summary>
    public string? GetCurrentUserFullName()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(CustomClaimTypes.FullName)?.Value;
    }

    /// <summary>
    /// Get the current user's role from claims.
    /// </summary>
    public string? GetCurrentUserRole()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
    }

    /// <summary>
    /// Check if the current user has a specific role.
    /// </summary>
    public bool HasRole(string role)
    {
        return GetCurrentUserRole()?.Equals(role, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    /// <summary>
    /// Check if the current user has any of the specified roles.
    /// </summary>
    public bool HasAnyRole(params string[] roles)
    {
        var userRole = GetCurrentUserRole();
        return !string.IsNullOrEmpty(userRole) && 
               roles.Any(r => r.Equals(userRole, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Check if the current user is authenticated.
    /// </summary>
    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

    /// <summary>
    /// Get all claims for the current user.
    /// </summary>
    public IEnumerable<Claim> GetAllClaims()
    {
        return _httpContextAccessor.HttpContext?.User?.Claims ?? Enumerable.Empty<Claim>();
    }

    /// <summary>
    /// Check if the current user is an Admin.
    /// </summary>
    public bool IsAdmin()
    {
        return HasRole(Roles.Admin);
    }

    /// <summary>
    /// Check if the current user is a Technician.
    /// </summary>
    public bool IsTechnician()
    {
        return HasRole(Roles.Technician);
    }

    /// <summary>
    /// Check if the current user is a Client.
    /// </summary>
    public bool IsClient()
    {
        return HasRole(Roles.Client);
    }
}
