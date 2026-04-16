using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Tests.Common;

public class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
        : base(options)
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
    public DbSet<AssetRequest> AssetRequests => Set<AssetRequest>();
    public DbSet<Maintenance> Maintenances => Set<Maintenance>();
    public DbSet<RepairingFirm> RepairingFirms => Set<RepairingFirm>();
    public DbSet<DiscountInfo> DiscountInfos => Set<DiscountInfo>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PurchasingOrderItem> PurchasingOrderItems => Set<PurchasingOrderItem>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AssetInforming> AssetInformings => Set<AssetInforming>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>().OwnsOne(a => a.Specifications, specBuilder =>
        {
            specBuilder.OwnsOne(s => s.Computer);
            specBuilder.OwnsOne(s => s.Server);
            specBuilder.OwnsOne(s => s.Networking);
            specBuilder.OwnsOne(s => s.Printing);
            specBuilder.OwnsOne(s => s.Furniture);
        });

        base.OnModelCreating(modelBuilder);
    }

    Task<int> IApplicationDbContext.SaveChangesAsync(CancellationToken cancellationToken)
    {
        return SaveChangesAsync(cancellationToken);
    }
}
