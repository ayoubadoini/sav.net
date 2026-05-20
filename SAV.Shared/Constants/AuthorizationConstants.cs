namespace SAV.Shared.Constants;

/// <summary>
/// Role constants used throughout the application.
/// Provides centralized definition to avoid magic strings.
/// </summary>
public static class Roles
{
    /// <summary>
    /// Administrator role. Full system access.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Technician role. Can perform technical operations and interventions.
    /// </summary>
    public const string Technician = "Technician";

    /// <summary>
    /// Client role. Can view own complaints and request service.
    /// </summary>
    public const string Client = "Client";

    /// <summary>
    /// Get all available roles.
    /// </summary>
    public static readonly string[] AllRoles = { Admin, Technician, Client };

    /// <summary>
    /// Verify if a role is valid.
    /// </summary>
    public static bool IsValidRole(string role) => AllRoles.Contains(role);
}

/// <summary>
/// Authorization policy names used in the application.
/// Provides centralized definition for policy-based authorization.
/// </summary>
public static class Policies
{
    /// <summary>
    /// Only Admin users can access.
    /// </summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>
    /// Only Technician users can access.
    /// </summary>
    public const string TechnicianOnly = "TechnicianOnly";

    /// <summary>
    /// Only Client users can access.
    /// </summary>
    public const string ClientOnly = "ClientOnly";

    /// <summary>
    /// Admin or Technician users can access.
    /// </summary>
    public const string AdminOrTechnician = "AdminOrTechnician";

    /// <summary>
    /// Admin or Client users can access.
    /// </summary>
    public const string AdminOrClient = "AdminOrClient";

    /// <summary>
    /// All authenticated users can access (used implicitly with [Authorize]).
    /// </summary>
    public const string Authenticated = "Authenticated";
}

/// <summary>
/// JWT claim type names used for custom claims.
/// </summary>
public static class CustomClaimTypes
{
    /// <summary>
    /// User role claim type.
    /// </summary>
    public const string Role = "role";

    /// <summary>
    /// User ID claim type.
    /// </summary>
    public const string UserId = "sub";

    /// <summary>
    /// User email claim type.
    /// </summary>
    public const string Email = "email";

    /// <summary>
    /// User full name claim type.
    /// </summary>
    public const string FullName = "fullName";

    /// <summary>
    /// User active status claim type.
    /// </summary>
    public const string IsActive = "isActive";
}

/// <summary>
/// Authorization requirement constants.
/// </summary>
public static class AuthorizationRequirements
{
    /// <summary>
    /// Claim value for active user.
    /// </summary>
    public const string ActiveUserClaimValue = "true";

    /// <summary>
    /// Minimum password length.
    /// </summary>
    public const int MinimumPasswordLength = 8;

    /// <summary>
    /// JWT token expiration in minutes.
    /// </summary>
    public const int TokenExpirationMinutes = 60;

    /// <summary>
    /// Refresh token expiration in days.
    /// </summary>
    public const int RefreshTokenExpirationDays = 7;
}
