using SAV.Domain.Common;

namespace SAV.Domain.Entities;

/// <summary>
/// Invoice entity representing a customer invoice.
/// </summary>
public class Invoice : BaseAuditableEntity
{
    /// <summary>
    /// Unique invoice number.
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Invoice issue date and time.
    /// </summary>
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Subtotal before taxes.
    /// </summary>
    public decimal SubTotal { get; set; }

    /// <summary>
    /// Tax amount.
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Total amount including taxes.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Payment status.
    /// </summary>
    public bool IsPaid { get; set; }

    /// <summary>
    /// Date when invoice was paid.
    /// </summary>
    public DateTime? PaidDate { get; set; }

    /// <summary>
    /// Foreign key to TechnicalIntervention.
    /// </summary>
    public int TechnicalInterventionId { get; set; }

    /// <summary>
    /// Navigation property to TechnicalIntervention.
    /// </summary>
    public TechnicalIntervention TechnicalIntervention { get; set; } = null!;

    /// <summary>
    /// Invoice line items.
    /// </summary>
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}
