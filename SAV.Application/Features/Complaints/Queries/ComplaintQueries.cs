using MediatR;
using SAV.Application.Features.Complaints.DTOs;

namespace SAV.Application.Features.Complaints.Queries;

public class GetComplaintByIdQuery : IRequest<ComplaintDto?>
{
    public int Id { get; set; }

    public GetComplaintByIdQuery(int id)
    {
        Id = id;
    }
}

public class GetAllComplaintsQuery : IRequest<IList<ComplaintDto>>
{
}

public class GetClientComplaintsQuery : IRequest<IList<ComplaintDto>>
{
    public int ClientId { get; set; }

    public GetClientComplaintsQuery(int clientId)
    {
        ClientId = clientId;
    }
}
