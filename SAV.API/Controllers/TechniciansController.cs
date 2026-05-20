using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAV.Domain.Entities;
using SAV.Domain.Enums;
using SAV.Infrastructure.Repositories;

namespace SAV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TechniciansController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public TechniciansController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var technicians = await _unitOfWork.Technicians
            .GetQueryable()
            .AsNoTracking()
            .Where(technician => !technician.IsDeleted)
            .Include(technician => technician.Interventions)
            .ToListAsync();

        return Ok(technicians.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var technician = await _unitOfWork.Technicians
            .GetQueryable()
            .AsNoTracking()
            .Where(item => !item.IsDeleted && item.Id == id)
            .Include(item => item.Interventions)
            .FirstOrDefaultAsync();

        if (technician == null)
        {
            return NotFound();
        }

        return Ok(MapToDto(technician));
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] TechnicianDto dto)
    {
        if (!IsValid(dto))
        {
            return BadRequest(new { message = "Full name, email, phone, and specialization are required." });
        }

        var technician = new Technician
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone.Trim(),
            Specialization = dto.Specialization.Trim(),
            IsActive = true
        };

        await _unitOfWork.Technicians.AddAsync(technician);
        await _unitOfWork.SaveChangesAsync();

        var createdTechnician = await _unitOfWork.Technicians
            .GetQueryable()
            .AsNoTracking()
            .Where(item => item.Id == technician.Id)
            .Include(item => item.Interventions)
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = technician.Id }, MapToDto(createdTechnician));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] TechnicianDto dto)
    {
        if (!IsValid(dto))
        {
            return BadRequest(new { message = "Full name, email, phone, and specialization are required." });
        }

        var technician = await _unitOfWork.Technicians
            .GetQueryable()
            .Include(item => item.Interventions)
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == id);

        if (technician == null)
        {
            return NotFound();
        }

        technician.FullName = dto.FullName.Trim();
        technician.Email = dto.Email.Trim();
        technician.Phone = dto.Phone.Trim();
        technician.Specialization = dto.Specialization.Trim();
        technician.IsActive = true;

        await _unitOfWork.Technicians.UpdateAsync(technician);
        await _unitOfWork.SaveChangesAsync();

        var updatedTechnician = await _unitOfWork.Technicians
            .GetQueryable()
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Include(item => item.Interventions)
            .FirstAsync();

        return Ok(MapToDto(updatedTechnician));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var technician = await _unitOfWork.Technicians
            .GetQueryable()
            .FirstOrDefaultAsync(item => !item.IsDeleted && item.Id == id);

        if (technician == null)
        {
            return NotFound();
        }

        await _unitOfWork.Technicians.DeleteAsync(technician);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    private static bool IsValid(TechnicianDto dto)
    {
        return !string.IsNullOrWhiteSpace(dto.FullName)
            && !string.IsNullOrWhiteSpace(dto.Email)
            && !string.IsNullOrWhiteSpace(dto.Phone)
            && !string.IsNullOrWhiteSpace(dto.Specialization);
    }

    private static TechnicianDto MapToDto(Technician technician)
    {
        var activeInterventions = technician.Interventions.Count(intervention => intervention.Status is InterventionStatus.Planned or InterventionStatus.InProgress);
        var completedInterventions = technician.Interventions.Count(intervention => intervention.Status == InterventionStatus.Completed);

        return new TechnicianDto
        {
            Id = technician.Id,
            FullName = technician.FullName,
            Email = technician.Email,
            Phone = technician.Phone,
            Specialization = technician.Specialization,
            Status = !technician.IsActive ? "Inactive" : activeInterventions > 0 ? "Busy" : "Available",
            InterventionsCompleted = completedInterventions
        };
    }

    public sealed class TechnicianDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int InterventionsCompleted { get; set; }
    }
}