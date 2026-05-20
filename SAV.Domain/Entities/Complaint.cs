using SAV.Domain.Common;
using SAV.Domain.Enums;

namespace SAV.Domain.Entities;

/// <summary>
/// Complaint entity representing a customer service complaint.
/// </summary>
public class Complaint : BaseAuditableEntity
{
    /// <summary>
    /// Complaint subject/title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed complaint description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the complaint.
    /// </summary>
    public ComplaintStatus Status { get; set; } = ComplaintStatus.Pending;

    /// <summary>
    /// Foreign key to Client.
    /// </summary>
    public int ClientId { get; set; }

    /// <summary>
    /// Foreign key to Article (optional).
    /// </summary>
    public int? ArticleId { get; set; }

    /// <summary>
    /// Navigation property to Client.
    /// </summary>
    public Client Client { get; set; } = null!;

    /// <summary>
    /// Navigation property to Article (optional).
    /// </summary>
    public Article? Article { get; set; }

    /// <summary>
    /// Related technical interventions.
    /// </summary>
    public ICollection<TechnicalIntervention> Interventions { get; set; } = new List<TechnicalIntervention>();
}
