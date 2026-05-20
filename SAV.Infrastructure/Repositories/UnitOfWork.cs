using SAV.Domain.Entities;

namespace SAV.Infrastructure.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Role> Roles { get; }
    IRepository<Client> Clients { get; }
    IRepository<Category> Categories { get; }
    IRepository<Article> Articles { get; }
    IComplaintRepository Complaints { get; }
    IRepository<Technician> Technicians { get; }
    IRepository<TechnicalIntervention> Interventions { get; }
    IRepository<Invoice> Invoices { get; }
    IRepository<InvoiceItem> InvoiceItems { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly SAV.Infrastructure.Persistence.SavDbContext _context;

    public IRepository<User> Users { get; private set; }
    public IRepository<Role> Roles { get; private set; }
    public IRepository<Client> Clients { get; private set; }
    public IRepository<Category> Categories { get; private set; }
    public IRepository<Article> Articles { get; private set; }
    public IComplaintRepository Complaints { get; private set; }
    public IRepository<Technician> Technicians { get; private set; }
    public IRepository<TechnicalIntervention> Interventions { get; private set; }
    public IRepository<Invoice> Invoices { get; private set; }
    public IRepository<InvoiceItem> InvoiceItems { get; private set; }

    public UnitOfWork(SAV.Infrastructure.Persistence.SavDbContext context)
    {
        _context = context;
        Users = new Repository<User>(_context);
        Roles = new Repository<Role>(_context);
        Clients = new Repository<Client>(_context);
        Categories = new Repository<Category>(_context);
        Articles = new Repository<Article>(_context);
        Complaints = new ComplaintRepository(_context);
        Technicians = new Repository<Technician>(_context);
        Interventions = new Repository<TechnicalIntervention>(_context);
        Invoices = new Repository<Invoice>(_context);
        InvoiceItems = new Repository<InvoiceItem>(_context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        await _context.Database.CommitTransactionAsync();
    }

    public async Task RollbackAsync()
    {
        await _context.Database.RollbackTransactionAsync();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
