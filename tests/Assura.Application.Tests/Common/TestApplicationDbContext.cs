using Assura.Application.Common.Interfaces;
using Assura.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Assura.Application.Tests.Common;

public class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Asset> Assets { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Division> Divisions { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<PurchasingOrder> PurchasingOrders { get; set; } = null!;
    public DbSet<GRN> GRNs { get; set; } = null!;
    public DbSet<QRN> QRNs { get; set; } = null!;
    public DbSet<TIN> TINs { get; set; } = null!;
    public DbSet<Transfer> Transfers { get; set; } = null!;
    public DbSet<Request> Requests { get; set; } = null!;
    public DbSet<AssetRequest> AssetRequests { get; set; } = null!;
    public DbSet<Maintenance> Maintenances { get; set; } = null!;
    public DbSet<RepairingFirm> RepairingFirms { get; set; } = null!;
    public DbSet<DiscountInfo> DiscountInfos { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<PurchasingOrderItem> PurchasingOrderItems { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<AssetInforming> AssetInformings { get; set; } = null!;
    public DbSet<DiscardedNote> DiscardedNotes { get; set; } = null!;
    public DbSet<QueueItem> QueueItems { get; set; } = null!;
    public DbSet<Buyer> Buyers { get; set; } = null!;
    public DbSet<AccPendingItem> AccPendingItems { get; set; } = null!;
    public DbSet<AccDiscardedItem> AccDiscardedItems { get; set; } = null!;
    public DbSet<Receipt> Receipts { get; set; } = null!;
    public DbSet<AccDiscardNote> AccDiscardNotes { get; set; } = null!;
    public DbSet<LostItem> LostItems { get; set; } = null!;
    public DbSet<UserDivisionRole> UserDivisionRoles { get; set; } = null!;
    public DbSet<TransferApproval> TransferApprovals { get; set; } = null!;
    public DbSet<CustomReport> CustomReports { get; set; } = null!;

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
