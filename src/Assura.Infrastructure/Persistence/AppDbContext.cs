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

        // Apply Soft Delete Global Query Filter
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private static System.Linq.Expressions.LambdaExpression ConvertFilterExpression(Type type)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(type, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var falseConstant = System.Linq.Expressions.Expression.Constant(false);
        var body = System.Linq.Expressions.Expression.Equal(property, falseConstant);
        return System.Linq.Expressions.Expression.Lambda(body, parameter);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = "System"; // Placeholder until Auth is implemented
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = "System"; // Placeholder
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = "System"; // Placeholder
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

