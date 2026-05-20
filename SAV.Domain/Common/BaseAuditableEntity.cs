namespace SAV.Domain.Common;

/// <summary>
/// Base class for entities that require audit tracking.
/// Provides automatic tracking of CreatedBy, CreatedDate, UpdatedBy, UpdatedDate.
/// </summary>
public abstract class BaseAuditableEntity
{
    /// <summary>
    /// Primary key identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// User ID who created this entity. Set automatically by interceptor.
    /// </summary>
    public int? CreatedBy { get; set; }

    /// <summary>
    /// Date and time (UTC) when this entity was created. Set automatically by interceptor.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User ID who last updated this entity. Set automatically by interceptor.
    /// </summary>
    public int? UpdatedBy { get; set; }

    /// <summary>
    /// Date and time (UTC) when this entity was last updated. Updated automatically by interceptor.
    /// </summary>
    public DateTime? UpdatedDate { get; set; }

    /// <summary>
    /// Soft delete flag. When true, entity is logically deleted.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time (UTC) when this entity was deleted (if applicable).
    /// </summary>
    public DateTime? DeletedDate { get; set; }
}
