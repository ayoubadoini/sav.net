namespace SAV.Shared.DTOs.Auth;

/// <summary>
/// Request model for creating an admin user account.
/// </summary>
public class CreateAdminRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
