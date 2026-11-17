using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAV.Application.Interfaces.Services;
using SAV.Domain.Entities;
using SAV.Domain.Enums;
using SAV.Infrastructure.Repositories;
using SAV.Shared.Constants;
using SAV.Shared.Interfaces;

namespace SAV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComplaintsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPdfService _pdfService;

    public ComplaintsController(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IPdfService pdfService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        IQueryable<Complaint> query = _unitOfWork.Complaints
            .GetQueryable()
            .AsNoTracking()
            .Where(complaint => !complaint.IsDeleted)
            .Include(complaint => complaint.Client)
                .ThenInclude(client => client.User)
            .Include(complaint => complaint.Article)
            .Include(complaint => complaint.Interventions)
                .ThenInclude(intervention => intervention.Technician);

        if (_currentUserService.IsClient())
        {
            var currentClient = await GetCurrentClientAsync();
            if (currentClient == null)
            {
                return Forbid();
            }

            query = query.Where(complaint => complaint.ClientId == currentClient.Id);
        }

        query = query.OrderByDescending(complaint => complaint.CreatedDate);

        var complaints = await query.ToListAsync();
        return Ok(complaints.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var complaint = await _unitOfWork.Complaints
            .GetQueryable()
            .AsNoTracking()
            .Where(item => !item.IsDeleted && item.Id == id)
            .Include(item => item.Client)
                .ThenInclude(client => client.User)
            .Include(item => item.Article)
            .Include(item => item.Interventions)
                .ThenInclude(intervention => intervention.Technician)
            .FirstOrDefaultAsync();

        if (complaint == null)
        {
            return NotFound();
        }

        if (_currentUserService.IsClient())
        {
            var currentClient = await GetCurrentClientAsync();
            if (currentClient == null || complaint.ClientId != currentClient.Id)
            {
                return Forbid();
            }
        }

        return Ok(MapToDto(complaint));
    }

    [HttpGet("{id}/pdf")]
    public async Task<ActionResult> DownloadComplaintPdf(int id)
    {
        var complaint = await _unitOfWork.Complaints
            .GetQueryable()
            .AsNoTracking()
            .Where(item => !item.IsDeleted && item.Id == id)
            .Include(item => item.Client)
                .ThenInclude(client => client.User)
            .Include(item => item.Article)
            .Include(item => item.Interventions)
                .ThenInclude(intervention => intervention.Technician)
            .FirstOrDefaultAsync();

        if (complaint == null)
        {
            return NotFound();
        }

        if (_currentUserService.IsClient())
        {
            var currentClient = await GetCurrentClientAsync();
            if (currentClient == null || complaint.ClientId != currentClient.Id)
            {
                return Forbid();
            }
        }

        var pdfBytes = await _pdfService.GenerateComplaintPdfAsync(complaint);
        var fileName = $"Complaint-{complaint.Id}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminOrClient)]
    public async Task<ActionResult> Create([FromBody] ComplaintDto dto)
    {
        if (!IsValid(dto))
        {
            return BadRequest(new { message = "Client, title, description, and priority are required." });
        }

        var isAdmin = _currentUserService.IsAdmin();
        var currentClient = await GetCurrentClientAsync();

        var clientId = dto.ClientId ?? currentClient?.Id ?? 0;
        if (clientId <= 0)
        {
            return BadRequest(new { message = "ClientId is required." });
        }

        if (!isAdmin && currentClient == null)
        {
            return BadRequest(new { message = "Current client profile was not found." });
        }

        if (!isAdmin && clientId != currentClient!.Id)
        {
            return BadRequest(new { message = "ClientId must match the authenticated client." });
        }

        var client = await _unitOfWork.Clients
            .GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == clientId);

        if (client == null)
        {
            return NotFound(new { message = "Client not found." });
        }

        var complaint = new Complaint
        {
            Title = dto.Title.Trim(),
            Description = EncodeDescription(dto.Priority, dto.Description),
            ClientId = client.Id,
            ArticleId = dto.ArticleId,
            Status = MapStatusFromText(dto.Status)
        };

        await _unitOfWork.Complaints.AddAsync(complaint);
        await _unitOfWork.SaveChangesAsync();

        if (dto.AssignedTechnicianId.HasValue)
        {
            var technicianExists = await _unitOfWork.Technicians
                .GetQueryable()
                .AnyAsync(technician => !technician.IsDeleted && technician.Id == dto.AssignedTechnicianId.Value);

            if (!technicianExists)
            {
                return NotFound(new { message = "Technician not found." });
            }

            var intervention = new TechnicalIntervention
            {
                ComplaintId = complaint.Id,
                TechnicianId = dto.AssignedTechnicianId,
                Date = DateTime.UtcNow,
                Description = $"Auto-assigned from complaint '{dto.Title.Trim()}'",
                LaborCost = 0m,
                Status = InterventionStatus.Planned
            };

            await _unitOfWork.Interventions.AddAsync(intervention);
            await _unitOfWork.SaveChangesAsync();
        }

        var createdComplaint = await _unitOfWork.Complaints
            .GetQueryable()
            .AsNoTracking()
            .Where(item => item.Id == complaint.Id)
            .Include(item => item.Client)
                .ThenInclude(client => client.User)
            .Include(item => item.Article)
            .Include(item => item.Interventions)
                .ThenInclude(intervention => intervention.Technician)
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = complaint.Id }, MapToDto(createdComplaint));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Policies.AdminOrTechnician)]
    public async Task<ActionResult> Update(int id, [FromBody] ComplaintDto dto)
    {
        if (!IsValid(dto))
        {
            return BadRequest(new { message = "Client, title, description, and priority are required." });
        }

        var complaint = await _unitOfWork.Complaints
            .GetQueryable()
            .Include(item => item.Client)
                .ThenInclude(client => client.User)
            .Include(item => item.Article)
            .Include(item => item.Interventions)
                .ThenInclude(intervention => intervention.Technician)
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == id);

        if (complaint == null)
        {
            return NotFound();
        }

        if (dto.ClientId.HasValue && dto.ClientId.Value != complaint.ClientId)
        {
            return BadRequest(new { message = "ClientId cannot be changed for an existing complaint." });
        }

        complaint.Title = dto.Title.Trim();
        complaint.Description = EncodeDescription(dto.Priority, dto.Description);
        complaint.ArticleId = dto.ArticleId ?? complaint.ArticleId;
        complaint.Status = MapStatusFromText(dto.Status);

        await _unitOfWork.Complaints.UpdateAsync(complaint);

        if (dto.AssignedTechnicianId.HasValue)
        {
            var technicianExists = await _unitOfWork.Technicians
                .GetQueryable()
                .AnyAsync(technician => !technician.IsDeleted && technician.Id == dto.AssignedTechnicianId.Value);

            if (!technicianExists)
            {
                return NotFound(new { message = "Technician not found." });
            }

            var intervention = await _unitOfWork.Interventions
                .GetQueryable()
                .FirstOrDefaultAsync(item => !item.IsDeleted && item.ComplaintId == complaint.Id);

            if (intervention == null)
            {
                intervention = new TechnicalIntervention
                {
                    ComplaintId = complaint.Id,
                    TechnicianId = dto.AssignedTechnicianId,
                    Date = DateTime.UtcNow,
                    Description = $"Auto-assigned from complaint '{dto.Title.Trim()}'",
                    LaborCost = 0m,
                    Status = InterventionStatus.Planned
                };

                await _unitOfWork.Interventions.AddAsync(intervention);
            }
            else
            {
                intervention.TechnicianId = dto.AssignedTechnicianId;
                intervention.Description = $"Auto-assigned from complaint '{dto.Title.Trim()}'";
                await _unitOfWork.Interventions.UpdateAsync(intervention);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        var updatedComplaint = await _unitOfWork.Complaints
            .GetQueryable()
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Include(item => item.Client)
                .ThenInclude(client => client.User)
            .Include(item => item.Article)
            .Include(item => item.Interventions)
                .ThenInclude(intervention => intervention.Technician)
            .FirstAsync();

        return Ok(MapToDto(updatedComplaint));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<ActionResult> Delete(int id)
    {
        var complaint = await _unitOfWork.Complaints
            .GetQueryable()
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == id);

        if (complaint == null)
        {
            return NotFound();
        }

        await _unitOfWork.Complaints.DeleteAsync(complaint);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("by-article/{articleId}")]
    public async Task<ActionResult> GetByArticle(int articleId)
    {
        var complaints = await _unitOfWork.Complaints
            .GetQueryable()
            .AsNoTracking()
            .Where(item => !item.IsDeleted && item.ArticleId == articleId)
            .Include(item => item.Client)
                .ThenInclude(c => c.User)
            .Include(item => item.Article)
            .Include(item => item.Interventions)
                .ThenInclude(intervention => intervention.Technician)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();

        return Ok(complaints.Select(MapToDto));
    }

    private async Task<Client?> GetCurrentClientAsync()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        if (!currentUserId.HasValue)
        {
            return null;
        }

        return await _unitOfWork.Clients
            .GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(client => !client.IsDeleted && client.UserId == currentUserId.Value);
    }

    private static bool IsValid(ComplaintDto dto)
    {
        return dto.ClientId is null || dto.ClientId > 0
            ? !string.IsNullOrWhiteSpace(dto.Title)
              && !string.IsNullOrWhiteSpace(dto.Description)
              && !string.IsNullOrWhiteSpace(dto.Priority)
            : false;
    }

    private static ComplaintDto MapToDto(Complaint complaint)
    {
        var (priority, description) = DecodeDescription(complaint.Description);
        var latestIntervention = complaint.Interventions
            .OrderByDescending(intervention => intervention.CreatedDate)
            .FirstOrDefault();

        return new ComplaintDto
        {
            Id = complaint.Id,
            ClientId = complaint.ClientId,
            Title = complaint.Title,
            Description = description,
            Priority = priority,
            Status = MapStatusToText(complaint.Status),
            ClientName = complaint.Client?.User?.FullName ?? string.Empty,
            CreatedAt = complaint.CreatedDate,
            AssignedTechnicianId = latestIntervention?.TechnicianId,
            ArticleId = complaint.ArticleId
        };
    }

    private static string EncodeDescription(string priority, string description)
    {
        return $"[priority:{priority.Trim()}] {description.Trim()}";
    }

    private static (string Priority, string Description) DecodeDescription(string description)
    {
        const string prefix = "[priority:";
        if (description.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            var closingIndex = description.IndexOf(']');
            if (closingIndex > prefix.Length)
            {
                var priority = description.Substring(prefix.Length, closingIndex - prefix.Length);
                var body = description.Substring(closingIndex + 1).TrimStart();
                return (priority, body);
            }
        }

        return ("Medium", description);
    }

    private static ComplaintStatus MapStatusFromText(string status)
    {
        return status.Trim().ToLowerInvariant() switch
        {
            "open" => ComplaintStatus.Pending,
            "pending" => ComplaintStatus.Pending,
            "inprogress" => ComplaintStatus.InProgress,
            "in progress" => ComplaintStatus.InProgress,
            "resolved" => ComplaintStatus.Resolved,
            "closed" => ComplaintStatus.Closed,
            "cancelled" => ComplaintStatus.Cancelled,
            _ => ComplaintStatus.Pending
        };
    }

    private static string MapStatusToText(ComplaintStatus status)
    {
        return status switch
        {
            ComplaintStatus.Pending => "Open",
            ComplaintStatus.InProgress => "InProgress",
            ComplaintStatus.Resolved => "Resolved",
            ComplaintStatus.Closed => "Closed",
            ComplaintStatus.Cancelled => "Cancelled",
            _ => status.ToString()
        };
    }

    public sealed class ComplaintDto
    {
        public int Id { get; set; }
        public int? ClientId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? AssignedTechnicianId { get; set; }
        public int? ArticleId { get; set; }
    }
}