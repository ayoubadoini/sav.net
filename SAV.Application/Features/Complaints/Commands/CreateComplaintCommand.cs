using MediatR;
using SAV.Application.Features.Complaints.DTOs;

namespace SAV.Application.Features.Complaints.Commands;

public class CreateComplaintCommand : IRequest<ComplaintDto>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ArticleId { get; set; }
    public int ClientId { get; set; }
}
