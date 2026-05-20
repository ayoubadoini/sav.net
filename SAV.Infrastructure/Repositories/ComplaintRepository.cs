using SAV.Domain.Entities;

namespace SAV.Infrastructure.Repositories;

public interface IComplaintRepository : IRepository<Complaint>
{
    Task<IEnumerable<Complaint>> GetByClientIdAsync(int clientId);
    Task<IEnumerable<Complaint>> GetByStatusAsync(SAV.Domain.Enums.ComplaintStatus status);
}

public class ComplaintRepository : Repository<Complaint>, IComplaintRepository
{
    public ComplaintRepository(SAV.Infrastructure.Persistence.SavDbContext context) : base(context) { }

    public async Task<IEnumerable<Complaint>> GetByClientIdAsync(int clientId)
    {
        return await Task.FromResult(_context.Complaints
            .Where(c => c.ClientId == clientId)
            .ToList());
    }

    public async Task<IEnumerable<Complaint>> GetByStatusAsync(SAV.Domain.Enums.ComplaintStatus status)
    {
        return await Task.FromResult(_context.Complaints
            .Where(c => c.Status == status)
            .ToList());
    }
}
