using Escale.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Escale.API.Data.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CurrentLevel).HasPrecision(18, 3);
        builder.Property(x => x.Capacity).HasPrecision(18, 3);
        builder.Property(x => x.ReorderLevel).HasPrecision(18, 3);
        builder.HasIndex(x => new { x.StationId, x.FuelTypeId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasOne(x => x.Organization).WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Station).WithMany(s => s.InventoryItems).HasForeignKey(x => x.StationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.FuelType).WithMany(f => f.InventoryItems).HasForeignKey(x => x.FuelTypeId).OnDelete(DeleteBehavior.Restrict);
    }
}
