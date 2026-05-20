using MediatR;
using SAV.Application.Features.Complaints.Commands;
using SAV.Application.Features.Complaints.DTOs;
using SAV.Domain.Entities;

namespace SAV.Application.Features.Complaints.Handlers;

public class CreateComplaintCommandHandler : IRequestHandler<CreateComplaintCommand, ComplaintDto>
{
    public async Task<ComplaintDto> Handle(CreateComplaintCommand request, CancellationToken cancellationToken)
    {
        // Placeholder - will be implemented with repository
        var complaint = new Complaint
        {
            Title = request.Title,
            Description = request.Description,
            ArticleId = request.ArticleId,
            ClientId = request.ClientId
        };

        var dto = new ComplaintDto
        {
            Id = 0,
            Title = complaint.Title,
            Description = complaint.Description,
            Status = complaint.Status.ToString(),
            CreatedAt = complaint.CreatedDate,
            ClientId = complaint.ClientId,
            ArticleId = complaint.ArticleId
        };

        await Task.CompletedTask;
        return dto;
    }
}
