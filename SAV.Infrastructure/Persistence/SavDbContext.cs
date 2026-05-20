using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SAV.Domain.Common;
using SAV.Domain.Entities;
using SAV.Shared.Interfaces;

namespace SAV.Infrastructure.Persistence;

/// <summary>
/// DbContext for the SAV application.
/// Implements automatic audit field population via SaveChangesAsync override.
/// </summary>
public class SavDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    private readonly ICurrentUserService? _currentUserService;

    public SavDbContext(DbContextOptions<SavDbContext> options, ICurrentUserService? currentUserService = null) 
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Role> ApplicationRoles { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Complaint> Complaints { get; set; }
    public DbSet<Technician> Technicians { get; set; }
    public DbSet<TechnicalIntervention> TechnicalInterventions { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    /// <summary>
    /// Override SaveChangesAsync to automatically populate audit fields.
    /// - On Create: Set CreatedBy, CreatedDate
    /// - On Update: Set UpdatedBy, UpdatedDate
    /// - On Delete: Set IsDeleted, DeletedDate (soft delete)
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditingInfo();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Override SaveChanges to automatically populate audit fields (sync version).
    /// </summary>
    public override int SaveChanges()
    {
        ApplyAuditingInfo();
        return base.SaveChanges();
    }

    /// <summary>
    /// Apply audit information to tracked entities.
    /// </summary>
    private void ApplyAuditingInfo()
    {
        var userId = _currentUserService?.GetCurrentUserId();
        var now = DateTime.UtcNow;

        var entries = ChangeTracker.Entries<BaseAuditableEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // On create, set CreatedBy and CreatedDate
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.CreatedDate = now;
                    entry.Entity.UpdatedDate = null;
                    entry.Entity.UpdatedBy = null;
                    break;

                case EntityState.Modified:
                    // On update, set UpdatedBy and UpdatedDate
                    // Protect CreatedBy and CreatedDate from being changed
                    entry.Property(e => e.CreatedBy).IsModified = false;
                    entry.Property(e => e.CreatedDate).IsModified = false;

                    entry.Entity.UpdatedBy = userId;
                    entry.Entity.UpdatedDate = now;
                    break;

                case EntityState.Deleted:
                    // Implement soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedDate = now;
                    entry.Entity.UpdatedBy = userId;
                    entry.Entity.UpdatedDate = now;
                    break;
            }
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        // Suppress the PendingModelChangesWarning for now (will be fixed with proper migration)
        optionsBuilder.ConfigureWarnings(w => 
            w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================================
        // ROLE CONFIGURATION
        // ============================================================
        modelBuilder.Entity<Role>().HasKey(r => r.Id);
        modelBuilder.Entity<Role>()
            .HasMany(r => r.Users)
            .WithOne(u => u.Role)
            .HasForeignKey(u => u.RoleId);

        // ============================================================
        // USER CONFIGURATION
        // ============================================================
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Client)
            .WithOne(c => c.User)
            .HasForeignKey<Client>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ============================================================
        // REFRESH TOKEN CONFIGURATION
        // ============================================================
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => new { rt.UserId, rt.IsRevoked, rt.ExpiresAt })
            .HasName("IX_RefreshToken_UserIdRevoked");

        // ============================================================
        // CLIENT CONFIGURATION
        // ============================================================
        modelBuilder.Entity<Client>()
            .HasMany(c => c.Complaints)
            .WithOne(co => co.Client)
            .HasForeignKey(co => co.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Client>()
            .HasQueryFilter(c => !c.IsDeleted);

        // ============================================================
        // CATEGORY & ARTICLE CONFIGURATION
        // ============================================================
        modelBuilder.Entity<Category>()
            .HasMany(c => c.Articles)
            .WithOne(a => a.Category)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Article>()
            .Property(a => a.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Article>()
            .HasMany(a => a.Complaints)
            .WithOne(c => c.Article)
            .HasForeignKey(c => c.ArticleId)
            .OnDelete(DeleteBehavior.SetNull);

        // ============================================================
        // COMPLAINT CONFIGURATION
        // ============================================================
        modelBuilder.Entity<Complaint>()
            .HasMany(c => c.Interventions)
            .WithOne(ti => ti.Complaint)
            .HasForeignKey(ti => ti.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filter for soft delete on Complaints
        modelBuilder.Entity<Complaint>()
            .HasQueryFilter(c => !c.IsDeleted);

        // ============================================================
        // TECHNICAL INTERVENTION CONFIGURATION
        // ============================================================
        modelBuilder.Entity<TechnicalIntervention>()
            .HasOne(ti => ti.Technician)
            .WithMany(t => t.Interventions)
            .HasForeignKey(ti => ti.TechnicianId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Technician>()
            .HasQueryFilter(t => !t.IsDeleted);

        modelBuilder.Entity<TechnicalIntervention>()
            .Property(ti => ti.LaborCost)
            .HasPrecision(18, 2);

        modelBuilder.Entity<TechnicalIntervention>()
            .HasOne(ti => ti.Invoice)
            .WithOne(i => i.TechnicalIntervention)
            .HasForeignKey<Invoice>(i => i.TechnicalInterventionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filter for soft delete
        modelBuilder.Entity<TechnicalIntervention>()
            .HasQueryFilter(ti => !ti.IsDeleted);

        // ============================================================
        // INVOICE CONFIGURATION
        // ============================================================
        modelBuilder.Entity<Invoice>()
            .Property(i => i.SubTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.TaxAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
            .HasMany(i => i.Items)
            .WithOne(ii => ii.Invoice)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Query filter for soft delete
        modelBuilder.Entity<Invoice>()
            .HasQueryFilter(i => !i.IsDeleted);

        // ============================================================
        // INVOICE ITEM CONFIGURATION
        // ============================================================
        modelBuilder.Entity<InvoiceItem>()
            .Property(ii => ii.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<InvoiceItem>()
            .HasOne(ii => ii.TechnicalIntervention)
            .WithMany(ti => ti.InvoiceItems)
            .HasForeignKey(ii => ii.TechnicalInterventionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query filter for soft delete - required because Invoice has a query filter
        modelBuilder.Entity<InvoiceItem>()
            .HasQueryFilter(ii => !ii.Invoice!.IsDeleted);

        // ============================================================
        // SEED INITIAL DATA
        // ============================================================
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "System Administrator" },
            new Role { Id = 2, Name = "Technician", Description = "Technical Service Provider" },
            new Role { Id = 3, Name = "Client", Description = "Customer" }
        );

        // ============================================================
        // AUDIT FIELD INDEXES
        // ============================================================
        // Create indexes on IsDeleted for efficient soft-delete queries
        modelBuilder.Entity<Complaint>().HasIndex(c => c.IsDeleted);
        modelBuilder.Entity<Complaint>().HasIndex(c => c.CreatedBy);

        modelBuilder.Entity<TechnicalIntervention>().HasIndex(ti => ti.IsDeleted);
        modelBuilder.Entity<Invoice>().HasIndex(i => i.IsDeleted);
        modelBuilder.Entity<Client>().HasIndex(c => c.IsDeleted);
        modelBuilder.Entity<Technician>().HasIndex(t => t.IsDeleted);
        modelBuilder.Entity<Article>().HasIndex(a => a.IsDeleted);
        modelBuilder.Entity<Category>().HasIndex(c => c.IsDeleted);
    }
}
