using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAV.Domain.Entities;
using SAV.Domain.Enums;
using SAV.Infrastructure.Repositories;
using SAV.Shared.Constants;

namespace SAV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InterventionsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public InterventionsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var interventions = await _unitOfWork.Interventions
            .GetQueryable()
            .AsNoTracking()
            .Where(intervention => !intervention.IsDeleted)
            .Include(intervention => intervention.Complaint)
                .ThenInclude(complaint => complaint.Client)
                    .ThenInclude(client => client.User)
            .Include(intervention => intervention.Technician)
            .ToListAsync();

        return Ok(interventions.Select(MapToDto));
    }

    [HttpGet("available-for-invoice")]
    public async Task<ActionResult> GetAvailableForInvoice()
    {
        var interventions = await _unitOfWork.Interventions
            .GetQueryable()
            .AsNoTracking()
            .Where(intervention => !intervention.IsDeleted)
            .Where(intervention => !_unitOfWork.Invoices.GetQueryable().Any(invoice => !invoice.IsDeleted && invoice.TechnicalInterventionId == intervention.Id))
            .Include(intervention => intervention.Complaint)
                .ThenInclude(complaint => complaint.Client)
                    .ThenInclude(client => client.User)
            .Include(intervention => intervention.Technician)
            .ToListAsync();

        return Ok(interventions.Select(MapToDto));
    }

    [HttpGet("/api/clients/{clientId}/interventions")]
    public async Task<ActionResult> GetByClientId(int clientId)
    {
        var interventions = await _unitOfWork.Interventions
            .GetQueryable()
            .AsNoTracking()
            .Where(intervention => !intervention.IsDeleted && intervention.Complaint.ClientId == clientId)
            .Include(intervention => intervention.Complaint)
                .ThenInclude(complaint => complaint.Client)
                    .ThenInclude(client => client.User)
            .Include(intervention => intervention.Technician)
            .ToListAsync();

        return Ok(interventions.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var intervention = await _unitOfWork.Interventions
            .GetQueryable()
            .AsNoTracking()
            .Where(item => !item.IsDeleted && item.Id == id)
            .Include(item => item.Complaint)
                .ThenInclude(complaint => complaint.Client)
                    .ThenInclude(client => client.User)
            .Include(item => item.Technician)
            .FirstOrDefaultAsync();

        if (intervention == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(intervention));
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminOrTechnician)]
    public async Task<ActionResult> Create([FromBody] InterventionDto dto)
    {
        if (!IsValid(dto))
        {
            return BadRequest(new { message = "Complaint, technician, scheduled date, and status are required." });
        }

        var complaint = await _unitOfWork.Complaints
            .GetQueryable()
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == dto.ComplaintId);

        if (complaint == null)
        {
            return NotFound(new { message = "Complaint not found." });
        }

        if (dto.TechnicianId.HasValue)
        {
            var technicianExists = await _unitOfWork.Technicians
                .GetQueryable()
                .AnyAsync(technician => !technician.IsDeleted && technician.Id == dto.TechnicianId.Value);

            if (!technicianExists)
            {
                return NotFound(new { message = "Technician not found." });
            }
        }

        var intervention = new TechnicalIntervention
        {
            ComplaintId = dto.ComplaintId,
            TechnicianId = dto.TechnicianId,
            Date = dto.ScheduledDate,
            Description = dto.Notes.Trim(),
            LaborCost = 0m,
            Status = MapStatusFromText(dto.Status)
        };

        await _unitOfWork.Interventions.AddAsync(intervention);
        await _unitOfWork.SaveChangesAsync();

        var createdIntervention = await _unitOfWork.Interventions
            .GetQueryable()
            .AsNoTracking()
            .Where(item => item.Id == intervention.Id)
            .Include(item => item.Complaint)
                .ThenInclude(complaint => complaint.Client)
                    .ThenInclude(client => client.User)
            .Include(item => item.Technician)
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = intervention.Id }, MapToDto(createdIntervention));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Policies.AdminOrTechnician)]
    public async Task<ActionResult> Update(int id, [FromBody] InterventionDto dto)
    {
        if (!IsValid(dto))
        {
            return BadRequest(new { message = "Complaint, technician, scheduled date, and status are required." });
        }

        var intervention = await _unitOfWork.Interventions
            .GetQueryable()
            .Include(item => item.Complaint)
                .ThenInclude(complaint => complaint.Client)
                    .ThenInclude(client => client.User)
            .Include(item => item.Technician)
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == id);

        if (intervention == null)
        {
            return NotFound();
        }

        var complaintExists = await _unitOfWork.Complaints
            .GetQueryable()
            .AnyAsync(item => !item.IsDeleted && item.Id == dto.ComplaintId);

        if (!complaintExists)
        {
            return NotFound(new { message = "Complaint not found." });
        }

        if (dto.TechnicianId.HasValue)
        {
            var technicianExists = await _unitOfWork.Technicians
                .GetQueryable()
                .AnyAsync(technician => !technician.IsDeleted && technician.Id == dto.TechnicianId.Value);

            if (!technicianExists)
            {
                return NotFound(new { message = "Technician not found." });
            }
        }

        intervention.ComplaintId = dto.ComplaintId;
        intervention.TechnicianId = dto.TechnicianId;
        intervention.Date = dto.ScheduledDate;
        intervention.Description = dto.Notes.Trim();
        intervention.Status = MapStatusFromText(dto.Status);

        await _unitOfWork.Interventions.UpdateAsync(intervention);
        await _unitOfWork.SaveChangesAsync();

        var updatedIntervention = await _unitOfWork.Interventions
            .GetQueryable()
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Include(item => item.Complaint)
                .ThenInclude(complaint => complaint.Client)
                    .ThenInclude(client => client.User)
            .Include(item => item.Technician)
            .FirstAsync();

        return Ok(MapToDto(updatedIntervention));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<ActionResult> Delete(int id)
    {
        var intervention = await _unitOfWork.Interventions
            .GetQueryable()
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == id);

        if (intervention == null)
        {
            return NotFound();
        }

        await _unitOfWork.Interventions.DeleteAsync(intervention);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    private static bool IsValid(InterventionDto dto)
    {
        return dto.ComplaintId > 0
            && dto.TechnicianId.HasValue
            && dto.ScheduledDate != default
            && !string.IsNullOrWhiteSpace(dto.Notes)
            && !string.IsNullOrWhiteSpace(dto.Status);
    }

    private static TechnicalInterventionDto MapToDto(TechnicalIntervention intervention)
    {
        return new TechnicalInterventionDto
        {
            Id = intervention.Id,
            ComplaintId = intervention.ComplaintId,
            ComplaintTitle = intervention.Complaint?.Title ?? string.Empty,
            TechnicianId = intervention.TechnicianId,
            TechnicianName = intervention.Technician?.FullName,
            ScheduledDate = intervention.Date,
            Notes = intervention.Description,
            Status = MapStatusToText(intervention.Status)
        };
    }

    private static InterventionStatus MapStatusFromText(string status)
    {
        return status.Trim().ToLowerInvariant() switch
        {
            "scheduled" => InterventionStatus.Planned,
            "planned" => InterventionStatus.Planned,
            "inprogress" => InterventionStatus.InProgress,
            "in progress" => InterventionStatus.InProgress,
            "completed" => InterventionStatus.Completed,
            "invoiced" => InterventionStatus.Invoiced,
            _ => InterventionStatus.Planned
        };
    }

    private static string MapStatusToText(InterventionStatus status)
    {
        return status switch
        {
            InterventionStatus.Planned => "Scheduled",
            InterventionStatus.InProgress => "InProgress",
            InterventionStatus.Completed => "Completed",
            InterventionStatus.Invoiced => "Invoiced",
            _ => status.ToString()
        };
    }

    public sealed class InterventionDto
    {
        public int ComplaintId { get; set; }
        public int? TechnicianId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public sealed class TechnicalInterventionDto
    {
        public int Id { get; set; }
        public int ComplaintId { get; set; }
        public string ComplaintTitle { get; set; } = string.Empty;
        public int? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}