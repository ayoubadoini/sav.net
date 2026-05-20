using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using SAV.Domain.Entities;

namespace SAV.Infrastructure.Services;

public interface IPdfService
{
    Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice);
}

public class PdfService : IPdfService
{
    public async Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice)
    {
        await Task.CompletedTask;

        using var ms = new MemoryStream();
        using var writer = new PdfWriter(ms);
        using var pdf = new PdfDocument(writer);
        using var document = new Document(pdf);

        var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        document.Add(new Paragraph("INVOICE").SetFont(titleFont).SetFontSize(20));
        document.Add(new Paragraph($"Invoice Number: {invoice.InvoiceNumber}"));
        document.Add(new Paragraph($"Date: {invoice.IssueDate:yyyy-MM-dd}"));
        document.Add(new Paragraph(" "));

        var table = new Table(3);
        table.AddHeaderCell("Description");
        table.AddHeaderCell("Unit Price");
        table.AddHeaderCell("Total");

        foreach (var item in invoice.Items)
        {
            table.AddCell(item.Description);
            table.AddCell($"${item.UnitPrice}");
            table.AddCell($"${item.TotalPrice}");
        }

        document.Add(table);
        document.Add(new Paragraph(" "));
        document.Add(new Paragraph($"Subtotal: ${invoice.SubTotal}"));
        document.Add(new Paragraph($"Tax: ${invoice.TaxAmount}"));
        document.Add(new Paragraph($"Total: ${invoice.TotalAmount}").SetFont(titleFont).SetFontSize(12));

        document.Flush();
        return ms.ToArray();
    }
}
