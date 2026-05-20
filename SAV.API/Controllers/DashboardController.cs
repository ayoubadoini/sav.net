using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAV.Domain.Enums;
using SAV.Infrastructure.Repositories;

namespace SAV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class DashboardController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("stats")]
    public async Task<ActionResult> GetStats()
    {
        var totalClients = await _unitOfWork.Clients.GetQueryable().CountAsync(client => !client.IsDeleted);
        var openComplaints = await _unitOfWork.Complaints.GetQueryable().CountAsync(complaint => complaint.Status == ComplaintStatus.Pending);
        var inProgressInterventions = await _unitOfWork.Interventions.GetQueryable().CountAsync(intervention => intervention.Status == InterventionStatus.InProgress);
        var totalRevenue = await _unitOfWork.Invoices.GetQueryable()
            .Where(invoice => invoice.IsPaid)
            .SumAsync(invoice => (decimal?)invoice.TotalAmount) ?? 0m;

        return Ok(new
        {
            totalClients,
            openComplaints,
            inProgressInterventions,
            totalRevenue,
            clientsChange = 0,
            complaintsChange = 0,
            interventionsChange = 0,
            revenueChange = 0
        });
    }

    [HttpGet("complaints-chart")]
    public async Task<ActionResult> GetComplaintsChart()
    {
        var monthStarts = Enumerable.Range(0, 6)
            .Select(offset => DateTime.UtcNow.AddMonths(-5 + offset))
            .Select(date => new DateTime(date.Year, date.Month, 1))
            .ToList();

        var complaints = await _unitOfWork.Complaints.GetQueryable()
            .Where(complaint => complaint.CreatedDate >= monthStarts.First())
            .Select(complaint => complaint.CreatedDate)
            .ToListAsync();

        var chart = monthStarts
            .Select(month => new
            {
                month = month.ToString("MMM"),
                complaints = complaints.Count(date => date.Year == month.Year && date.Month == month.Month),
                trend = "up"
            })
            .ToList();

        return Ok(chart);
    }

    [HttpGet("revenue-chart")]
    public async Task<ActionResult> GetRevenueChart()
    {
        var monthStarts = Enumerable.Range(0, 6)
            .Select(offset => DateTime.UtcNow.AddMonths(-5 + offset))
            .Select(date => new DateTime(date.Year, date.Month, 1))
            .ToList();

        var invoices = await _unitOfWork.Invoices.GetQueryable()
            .Where(invoice => invoice.IsPaid && invoice.IssueDate >= monthStarts.First())
            .Select(invoice => new { invoice.IssueDate, invoice.TotalAmount })
            .ToListAsync();

        var chart = monthStarts
            .Select(month => new
            {
                month = month.ToString("MMM"),
                revenue = invoices
                    .Where(invoice => invoice.IssueDate.Year == month.Year && invoice.IssueDate.Month == month.Month)
                    .Sum(invoice => invoice.TotalAmount)
            })
            .ToList();

        return Ok(chart);
    }

    [HttpGet("status-chart")]
    public async Task<ActionResult> GetStatusChart()
    {
        var complaints = await _unitOfWork.Complaints.GetQueryable()
            .Select(complaint => complaint.Status)
            .ToListAsync();

        var chart = complaints
            .GroupBy(status => status)
            .Select(group => new
            {
                status = MapComplaintStatus(group.Key),
                count = group.Count(),
                color = GetStatusColor(group.Key)
            })
            .ToList();

        return Ok(chart);
    }

    [HttpGet("technician-chart")]
    public async Task<ActionResult> GetTechnicianChart()
    {
        var chart = await _unitOfWork.Interventions.GetQueryable()
            .Where(intervention => intervention.TechnicianId != null)
            .GroupBy(intervention => intervention.TechnicianId)
            .Select(group => new
            {
                TechnicianId = group.Key,
                Interventions = group.Count()
            })
            .Join(
                _unitOfWork.Technicians.GetQueryable().AsNoTracking(),
                item => item.TechnicianId,
                technician => technician.Id,
                (item, technician) => new
                {
                    name = technician.FullName,
                    interventions = item.Interventions
                })
            .OrderByDescending(item => item.interventions)
            .ToListAsync();

        return Ok(chart);
    }

    private static string MapComplaintStatus(ComplaintStatus status)
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

    private static string GetStatusColor(ComplaintStatus status)
    {
        return status switch
        {
            ComplaintStatus.Pending => "#F59E0B",
            ComplaintStatus.InProgress => "#3B82F6",
            ComplaintStatus.Resolved => "#10B981",
            ComplaintStatus.Closed => "#6B7280",
            ComplaintStatus.Cancelled => "#EF4444",
            _ => "#6B7280"
        };
    }
}