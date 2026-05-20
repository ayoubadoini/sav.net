using System.Security.Claims;

namespace SAV.Shared.Interfaces;

/// <summary>
/// Interface for accessing the current authenticated user's information.
/// Provides methods to retrieve user ID, email, role, and claims.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Get the current user's ID.
    /// </summary>
    int? GetCurrentUserId();

    /// <summary>
    /// Get the current user's email address.
    /// </summary>
    string? GetCurrentUserEmail();

    /// <summary>
    /// Get the current user's full name.
    /// </summary>
    string? GetCurrentUserFullName();

    /// <summary>
    /// Get the current user's role.
    /// </summary>
    string? GetCurrentUserRole();

    /// <summary>
    /// Check if the current user has a specific role.
    /// </summary>
    bool HasRole(string role);

    /// <summary>
    /// Check if the current user has any of the specified roles.
    /// </summary>
    bool HasAnyRole(params string[] roles);

    /// <summary>
    /// Check if the user is authenticated.
    /// </summary>
    bool IsAuthenticated();

    /// <summary>
    /// Get all claims for the current user.
    /// </summary>
    IEnumerable<Claim> GetAllClaims();

    /// <summary>
    /// Check if the current user is an Admin.
    /// </summary>
    bool IsAdmin();

    /// <summary>
    /// Check if the current user is a Technician.
    /// </summary>
    bool IsTechnician();

    /// <summary>
    /// Check if the current user is a Client.
    /// </summary>
    bool IsClient();
}
