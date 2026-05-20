using MediatR;
using SAV.Application.DTOs.Auth;
using SAV.Domain.Entities;

namespace SAV.Application.Features.Complaints.DTOs;

public class ComplaintDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ClientId { get; set; }
    public int? ArticleId { get; set; }
    public string ArticleName { get; set; } = string.Empty;
}
