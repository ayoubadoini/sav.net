namespace SAV.Domain.Entities;

public class InvoiceItem
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;

    // Foreign Keys
    public int InvoiceId { get; set; }
    public int TechnicalInterventionId { get; set; }

    // Navigation properties
    public Invoice Invoice { get; set; } = null!;
    public TechnicalIntervention TechnicalIntervention { get; set; } = null!;
}
