namespace SAV.Domain.Entities;

/// <summary>
/// Represents a refresh token issued to a user for obtaining new access tokens.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }

    /// <summary>
    /// The actual refresh token string (JWT).
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The user ID associated with this refresh token.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The date and time when this refresh token expires (UTC).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Indicates whether this refresh token has been revoked.
    /// </summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>
    /// The date and time when this refresh token was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual User? User { get; set; }
}
