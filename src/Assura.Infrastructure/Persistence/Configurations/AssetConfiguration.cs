using Assura.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Assura.Infrastructure.Persistence.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AssetCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(a => a.AssetCode)
            .IsUnique();

        builder.HasOne(a => a.Category)
            .WithMany(c => c.Assets)
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Division)
            .WithMany(d => d.Assets)
            .HasForeignKey(a => a.DivisionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Product)
            .WithMany(p => p.Assets)
            .HasForeignKey(a => a.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Supplier)
            .WithMany(s => s.Assets)
            .HasForeignKey(a => a.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AssignedUser)
            .WithMany(u => u.AssignedAssets)
            .HasForeignKey(a => a.AssignedUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
