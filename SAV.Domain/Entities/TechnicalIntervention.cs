using SAV.Domain.Common;
using SAV.Domain.Enums;

namespace SAV.Domain.Entities;

/// <summary>
/// TechnicalIntervention entity representing a technical service intervention.
/// </summary>
public class TechnicalIntervention : BaseAuditableEntity
{
    /// <summary>
    /// Intervention date and time.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Intervention description and notes.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Labor cost for this intervention.
    /// </summary>
    public decimal LaborCost { get; set; }

    /// <summary>
    /// Current status of the intervention.
    /// </summary>
    public InterventionStatus Status { get; set; } = InterventionStatus.Planned;

    /// <summary>
    /// Date when intervention was completed.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Foreign key to Complaint.
    /// </summary>
    public int ComplaintId { get; set; }

    /// <summary>
    /// Foreign key to Technician.
    /// </summary>
    public int? TechnicianId { get; set; }

    /// <summary>
    /// Navigation property to Complaint.
    /// </summary>
    public Complaint Complaint { get; set; } = null!;

    /// <summary>
    /// Navigation property to Technician.
    /// </summary>
    public Technician? Technician { get; set; }

    /// <summary>
    /// Related invoice items.
    /// </summary>
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

    /// <summary>
    /// Related invoice.
    /// </summary>
    public Invoice? Invoice { get; set; }

    /// <summary>
    /// Warranty tracking is not stored on Article in the new product model.
    /// </summary>
    public bool IsUnderWarranty => false;
}
