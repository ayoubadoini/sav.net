using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAV.Domain.Entities;
using SAV.Infrastructure.Repositories;

namespace SAV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public InvoicesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var invoices = await _unitOfWork.Invoices
            .GetQueryable()
            .AsNoTracking()
            .Where(invoice => !invoice.IsDeleted)
            .Include(invoice => invoice.TechnicalIntervention)
                .ThenInclude(intervention => intervention.Complaint)
                    .ThenInclude(complaint => complaint.Client)
                        .ThenInclude(client => client.User)
            .Include(invoice => invoice.Items)
            .ToListAsync();

        return Ok(invoices.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var invoice = await _unitOfWork.Invoices
            .GetQueryable()
            .AsNoTracking()
            .Where(item => !item.IsDeleted && item.Id == id)
            .Include(item => item.TechnicalIntervention)
                .ThenInclude(intervention => intervention.Complaint)
                    .ThenInclude(complaint => complaint.Client)
                        .ThenInclude(client => client.User)
            .Include(item => item.Items)
            .FirstOrDefaultAsync();

        if (invoice == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(invoice));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] InvoiceDto dto)
    {
        if (!IsValid(dto))
        {
            return BadRequest(new { message = "Client, intervention, total amount, due date, and status are required." });
        }

        var interventionId = dto.TechnicalInterventionId!.Value;

        var intervention = await _unitOfWork.Interventions
            .GetQueryable()
            .Include(item => item.Complaint)
                .ThenInclude(complaint => complaint.Client)
                    .ThenInclude(client => client.User)
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == interventionId);

        if (intervention == null)
        {
            return NotFound(new { message = "Technical intervention not found." });
        }

        var existingInvoice = await _unitOfWork.Invoices
            .GetQueryable()
            .AsNoTracking()
            .AnyAsync(item => !item.IsDeleted && item.TechnicalInterventionId == interventionId);

        if (existingInvoice)
        {
            return Conflict(new { message = "This technical intervention already has an invoice." });
        }

        if (intervention.Complaint.ClientId != dto.ClientId)
        {
            return BadRequest(new { message = "Selected client does not match the intervention complaint client." });
        }

        var invoice = new Invoice
        {
            InvoiceNumber = GenerateInvoiceNumber(),
            IssueDate = DateTime.UtcNow,
            SubTotal = dto.TotalAmount,
            TaxAmount = 0m,
            TotalAmount = dto.TotalAmount,
            IsPaid = IsPaidStatus(dto.Status),
            PaidDate = IsPaidStatus(dto.Status) ? DateTime.UtcNow : null,
            TechnicalInterventionId = interventionId
        };

        await _unitOfWork.Invoices.AddAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        var createdInvoice = await _unitOfWork.Invoices
            .GetQueryable()
            .AsNoTracking()
            .Where(item => item.Id == invoice.Id)
            .Include(item => item.TechnicalIntervention)
                .ThenInclude(interventionItem => interventionItem.Complaint)
                    .ThenInclude(complaint => complaint.Client)
                        .ThenInclude(client => client.User)
            .Include(item => item.Items)
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, MapToDto(createdInvoice));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] InvoiceDto dto)
    {
        if (!IsValid(dto))
        {
            return BadRequest(new { message = "Client, intervention, total amount, due date, and status are required." });
        }

        var interventionId = dto.TechnicalInterventionId!.Value;

        var invoice = await _unitOfWork.Invoices
            .GetQueryable()
            .Include(item => item.TechnicalIntervention)
                .ThenInclude(intervention => intervention.Complaint)
                    .ThenInclude(complaint => complaint.Client)
                        .ThenInclude(client => client.User)
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == id);

        if (invoice == null)
        {
            return NotFound();
        }

        var intervention = await _unitOfWork.Interventions
            .GetQueryable()
            .Include(item => item.Complaint)
                .ThenInclude(complaint => complaint.Client)
                    .ThenInclude(client => client.User)
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == interventionId);

        if (intervention == null)
        {
            return NotFound(new { message = "Technical intervention not found." });
        }

        var invoiceForIntervention = await _unitOfWork.Invoices
            .GetQueryable()
            .AsNoTracking()
            .AnyAsync(item => !item.IsDeleted && item.Id != id && item.TechnicalInterventionId == interventionId);

        if (invoiceForIntervention)
        {
            return Conflict(new { message = "This technical intervention already has an invoice." });
        }

        if (intervention.Complaint.ClientId != dto.ClientId)
        {
            return BadRequest(new { message = "Selected client does not match the intervention complaint client." });
        }

        invoice.TotalAmount = dto.TotalAmount;
        invoice.SubTotal = dto.TotalAmount;
        invoice.TaxAmount = 0m;
        invoice.IsPaid = IsPaidStatus(dto.Status);
        invoice.PaidDate = invoice.IsPaid ? DateTime.UtcNow : null;
        invoice.TechnicalInterventionId = interventionId;

        await _unitOfWork.Invoices.UpdateAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        var updatedInvoice = await _unitOfWork.Invoices
            .GetQueryable()
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Include(item => item.TechnicalIntervention)
                .ThenInclude(interventionItem => interventionItem.Complaint)
                    .ThenInclude(complaint => complaint.Client)
                        .ThenInclude(client => client.User)
            .Include(item => item.Items)
            .FirstAsync();

        return Ok(MapToDto(updatedInvoice));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var invoice = await _unitOfWork.Invoices
            .GetQueryable()
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == id);

        if (invoice == null)
        {
            return NotFound();
        }

        await _unitOfWork.Invoices.DeleteAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    private static bool IsValid(InvoiceDto dto)
    {
        return dto.ClientId > 0
            && dto.TechnicalInterventionId.HasValue
            && dto.TotalAmount >= 0m
            && dto.DueDate != default
            && !string.IsNullOrWhiteSpace(dto.Status);
    }

    private static bool IsPaidStatus(string status)
    {
        return status.Trim().Equals("Paid", StringComparison.OrdinalIgnoreCase);
    }

    private static string GenerateInvoiceNumber()
    {
        return $"INV-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    }

    private static InvoiceDto MapToDto(Invoice invoice)
    {
        var dueDate = invoice.IssueDate.AddDays(30);
        var clientName = invoice.TechnicalIntervention?.Complaint?.Client?.User?.FullName ?? string.Empty;

        return new InvoiceDto
        {
            Id = invoice.Id,
            ClientId = invoice.TechnicalIntervention?.Complaint?.ClientId ?? 0,
            TechnicalInterventionId = invoice.TechnicalInterventionId,
            TotalAmount = invoice.TotalAmount,
            DueDate = dueDate,
            Status = GetStatusText(invoice, dueDate),
            InvoiceNumber = invoice.InvoiceNumber,
            ClientName = clientName,
            Amount = invoice.TotalAmount,
            Date = invoice.IssueDate,
            Items = invoice.Items.Count,
            InterventionId = invoice.TechnicalInterventionId
        };
    }

    private static string GetStatusText(Invoice invoice, DateTime dueDate)
    {
        if (invoice.IsPaid)
        {
            return "Paid";
        }

        return dueDate < DateTime.UtcNow ? "Overdue" : "Sent";
    }

    public sealed class InvoiceDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int? TechnicalInterventionId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;

        public string InvoiceNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public int Items { get; set; }
        public int? InterventionId { get; set; }
    }
}