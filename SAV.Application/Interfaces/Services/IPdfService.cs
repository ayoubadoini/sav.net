using SAV.Domain.Entities;

namespace SAV.Application.Interfaces.Services;

/// <summary>
/// Service for generating professional PDF documents for invoices, interventions, and dashboard exports
/// Uses QuestPDF for modern, fluent PDF generation
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Generates a professional invoice PDF with company header, invoice details, items table, totals, and QR code
    /// </summary>
    /// <param name="invoice">Invoice entity to generate PDF from</param>
    /// <returns>PDF file as byte array</returns>
    Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice);

    /// <summary>
    /// Generates a complaint PDF with customer, status, article, and intervention summary
    /// </summary>
    /// <param name="complaint">Complaint entity to generate PDF from</param>
    /// <returns>PDF file as byte array</returns>
    Task<byte[]> GenerateComplaintPdfAsync(Complaint complaint);

    /// <summary>
    /// Generates a technical intervention PDF with intervention details, labor costs, and completion status
    /// </summary>
    /// <param name="intervention">TechnicalIntervention entity to generate PDF from</param>
    /// <returns>PDF file as byte array</returns>
    Task<byte[]> GenerateInterventionPdfAsync(TechnicalIntervention intervention);

    /// <summary>
    /// Generates a dashboard statistics PDF with KPIs, charts, and technician performance
    /// </summary>
    /// <param name="dashboardData">Dashboard statistics data (KPI metrics, revenue, complaint status, technician stats)</param>
    /// <returns>PDF file as byte array</returns>
    Task<byte[]> GenerateDashboardPdfAsync(DashboardExportData dashboardData);
}

/// <summary>
/// DTO for dashboard export data containing statistics and KPIs
/// </summary>
public class DashboardExportData
{
    public DateTime ExportDate { get; set; } = DateTime.Now;
    
    // KPI Metrics
    public int TotalComplaints { get; set; }
    public int ResolvedComplaints { get; set; }
    public int PendingComplaints { get; set; }
    public decimal TotalRevenue { get; set; }
    
    // Interventions
    public int TotalInterventions { get; set; }
    public int CompletedInterventions { get; set; }
    public decimal AverageInterventionCost { get; set; }
    
    // Invoices
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public decimal OutstandingAmount { get; set; }
    
    // Technician Performance
    public List<TechnicianPerformanceDto> TechnicianStats { get; set; } = new();
    
    // Status Distribution
    public Dictionary<string, int> ComplaintStatusDistribution { get; set; } = new();
    public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
}

/// <summary>
/// DTO for technician performance statistics
/// </summary>
public class TechnicianPerformanceDto
{
    public int TechnicianId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int CompletedInterventions { get; set; }
    public decimal TotalRevenue { get; set; }
    public double AverageRating { get; set; }
    public string Specialization { get; set; } = string.Empty;
}
