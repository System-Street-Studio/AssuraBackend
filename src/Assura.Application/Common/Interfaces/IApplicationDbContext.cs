using Assura.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Assura.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Asset> Assets { get; }
    DbSet<Category> Categories { get; }
    DbSet<Division> Divisions { get; }
    DbSet<Product> Products { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<User> Users { get; }
    DbSet<PurchasingOrder> PurchasingOrders { get; }
    DbSet<GRN> GRNs { get; }
    DbSet<QRN> QRNs { get; }
    DbSet<TIN> TINs { get; }
    DbSet<Transfer> Transfers { get; }
    DbSet<Request> Requests { get; }
    DbSet<Maintenance> MaintenanceRecords { get; }
    DbSet<RepairingFirm> RepairingFirms { get; }
    DbSet<DiscountInfo> DiscountInfos { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<PurchasingOrderItem> PurchasingOrderItems { get; }
    DbSet<Notification> Notifications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
