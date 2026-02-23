using Assura.Application.Common.Interfaces;
using Assura.Domain.Common;
using Assura.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Assura.Infrastructure.Persistence;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Division> Divisions => Set<Division>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PurchasingOrder> PurchasingOrders => Set<PurchasingOrder>();
    public DbSet<GRN> GRNs => Set<GRN>();
    public DbSet<QRN> QRNs => Set<QRN>();
    public DbSet<TIN> TINs => Set<TIN>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<Maintenance> MaintenanceRecords => Set<Maintenance>();
    public DbSet<RepairingFirm> RepairingFirms => Set<RepairingFirm>();
    public DbSet<DiscountInfo> DiscountInfos => Set<DiscountInfo>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    // entry.Entity.CreatedBy = _currentUserService.UserId; // Placeholder for when auth is ready
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    // entry.Entity.UpdatedBy = _currentUserService.UserId; // Placeholder
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

