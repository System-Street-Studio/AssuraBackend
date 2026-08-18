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
    private readonly ICurrentUserService _currentUserService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
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
    public DbSet<GIN> GINs => Set<GIN>();
    public DbSet<TIN> TINs => Set<TIN>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<AssetRequest> AssetRequests => Set<AssetRequest>();
    public DbSet<Maintenance> Maintenances => Set<Maintenance>();
    public DbSet<RepairingFirm> RepairingFirms => Set<RepairingFirm>();
    public DbSet<DiscountInfo> DiscountInfos => Set<DiscountInfo>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PurchasingOrderItem> PurchasingOrderItems => Set<PurchasingOrderItem>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AssetInforming> AssetInformings => Set<AssetInforming>();
    public DbSet<DiscardedNote> DiscardedNotes => Set<DiscardedNote>();
    public DbSet<QueueItem> QueueItems => Set<QueueItem>();
    public DbSet<Buyer> Buyers => Set<Buyer>();
    public DbSet<AccPendingItem> AccPendingItems => Set<AccPendingItem>();
    public DbSet<AccDiscardedItem> AccDiscardedItems => Set<AccDiscardedItem>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<AccDiscardNote> AccDiscardNotes => Set<AccDiscardNote>();
    public DbSet<LostItem> LostItems => Set<LostItem>();
    public DbSet<UserDivisionRole> UserDivisionRoles => Set<UserDivisionRole>();
    public DbSet<TransferApproval> TransferApprovals => Set<TransferApproval>();
    public DbSet<CustomReport> CustomReports => Set<CustomReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
                modelBuilder.Entity(entityType.ClrType).Property(nameof(BaseEntity.Version)).IsConcurrencyToken();
            }
        }
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
                    entry.Entity.CreatedBy = _currentUserService.UserId ?? "System";
                    entry.Entity.Version = 1;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = _currentUserService.UserId ?? "System";
                    entry.Entity.Version++;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = _currentUserService.UserId ?? "System";
                    entry.Entity.Version++;
                    break;
            }
        }

        var auditEntries = OnBeforeSaveChanges();

        var result = await base.SaveChangesAsync(cancellationToken);

        await OnAfterSaveChanges(auditEntries, cancellationToken);

        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
            {
                continue;
            }

            var auditEntry = new AuditEntry(entry)
            {
                EntityName = entry.Entity.GetType().Name,
                CreatedBy = _currentUserService.UserId ?? "System",
                Action = entry.State switch
                {
                    EntityState.Added => "Create",
                    EntityState.Deleted => "Delete",
                    EntityState.Modified => (entry.Entity is BaseEntity &&
                                             entry.CurrentValues.GetValue<bool>(nameof(BaseEntity.IsDeleted)) &&
                                             !entry.OriginalValues.GetValue<bool>(nameof(BaseEntity.IsDeleted)))
                                             ? "Delete" : "Update",
                    _ => entry.State.ToString()
                }
            };

            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;

                if (property.Metadata.IsPrimaryKey())
                {
                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                    }
                    else
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                    }
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }
        }

        return auditEntries;
    }

    private async Task OnAfterSaveChanges(List<AuditEntry> auditEntries, CancellationToken cancellationToken)
    {
        if (auditEntries == null || auditEntries.Count == 0)
        {
            return;
        }

        foreach (var auditEntry in auditEntries)
        {
            foreach (var prop in auditEntry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                }
                else
                {
                    auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                }
            }

            var log = auditEntry.ToAuditLog();
            log.CreatedAt = DateTime.UtcNow;
            log.CreatedBy = _currentUserService.UserId ?? "System";
            log.Version = 1;

            AuditLogs.Add(log);
        }

        await base.SaveChangesAsync(cancellationToken);
    }
}
