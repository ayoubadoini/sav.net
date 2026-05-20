namespace SAV.Domain.Common;

/// <summary>
/// Interface for entities that support audit tracking.
/// Implemented by BaseAuditableEntity and its derived classes.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// User ID who created this entity.
    /// </summary>
    int? CreatedBy { get; set; }

    /// <summary>
    /// Date and time (UTC) when entity was created.
    /// </summary>
    DateTime CreatedDate { get; set; }

    /// <summary>
    /// User ID who last updated this entity.
    /// </summary>
    int? UpdatedBy { get; set; }

    /// <summary>
    /// Date and time (UTC) when entity was last updated.
    /// </summary>
    DateTime? UpdatedDate { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Date and time (UTC) when entity was deleted.
    /// </summary>
    DateTime? DeletedDate { get; set; }
}

/// <summary>
/// Marker interface for entities that require audit tracking.
/// Used by EF Core SaveChangesAsync interceptor to automatically populate audit fields.
/// </summary>
public interface IAuditable
{
}
