using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SAV.Application.Interfaces.Services;
using SAV.Domain.Entities;

namespace SAV.Infrastructure.Services;

public class PdfService : IPdfService
{
    private const string CompanyName = "SAV Manager";

    public PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Content()
                    .Column(column =>
                    {
                        column.Item().Text(CompanyName).FontSize(24).Bold();
                        column.Item().Text($"Invoice #{invoice.InvoiceNumber}");
                        column.Item().Text($"Total: ${invoice.TotalAmount:F2}");
                    });
            });
        });

        var pdf = document.GeneratePdf();
        return await Task.FromResult(pdf);
    }

    public async Task<byte[]> GenerateComplaintPdfAsync(Complaint complaint)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Content()
                    .Column(column =>
                    {
                        column.Item().Text(CompanyName).FontSize(24).Bold();
                        column.Item().Text($"Complaint #{complaint.Id}");
                        column.Item().Text($"Title: {complaint.Title}");
                        column.Item().Text($"Status: {complaint.Status}");
                        column.Item().Text($"Client: {complaint.Client?.User?.FullName ?? "N/A"}");
                        column.Item().Text($"Description: {complaint.Description}");
                        column.Item().Text($"Article: {complaint.Article?.Name ?? "N/A"}");
                        column.Item().Text($"Interventions: {complaint.Interventions.Count}");
                    });
            });
        });

        var pdf = document.GeneratePdf();
        return await Task.FromResult(pdf);
    }

    public async Task<byte[]> GenerateInterventionPdfAsync(TechnicalIntervention intervention)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Content()
                    .Column(column =>
                    {
                        column.Item().Text(CompanyName).FontSize(24).Bold();
                        column.Item().Text($"Intervention ID: {intervention.Id}");
                        column.Item().Text($"Status: {intervention.Status}");
                        column.Item().Text($"Cost: ${intervention.LaborCost:F2}");
                    });
            });
        });

        var pdf = document.GeneratePdf();
        return await Task.FromResult(pdf);
    }

    public async Task<byte[]> GenerateDashboardPdfAsync(DashboardExportData dashboardData)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Content()
                    .Column(column =>
                    {
                        column.Item().Text(CompanyName).FontSize(24).Bold();
                        column.Item().Text("Dashboard Report");
                        column.Item().Text($"Total Complaints: {dashboardData.TotalComplaints}");
                        column.Item().Text($"Resolved: {dashboardData.ResolvedComplaints}");
                        column.Item().Text($"Revenue: ${dashboardData.TotalRevenue:F2}");
                    });
            });
        });

        var pdf = document.GeneratePdf();
        return await Task.FromResult(pdf);
    }
}
