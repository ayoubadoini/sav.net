using SAV.Domain.Common;

namespace SAV.Domain.Entities;

/// <summary>
/// Technician entity representing a service technician.
/// </summary>
public class Technician : BaseAuditableEntity
{
    /// <summary>
    /// Technician full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Technician email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Technician phone number.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Technician specialization/expertise.
    /// </summary>
    public string Specialization { get; set; } = string.Empty;

    /// <summary>
    /// Active status.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Technical interventions assigned to this technician.
    /// </summary>
    public ICollection<TechnicalIntervention> Interventions { get; set; } = new List<TechnicalIntervention>();
}
