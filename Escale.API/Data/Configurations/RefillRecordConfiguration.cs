using Escale.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Escale.API.Data.Configurations;

public class RefillRecordConfiguration : IEntityTypeConfiguration<RefillRecord>
{
    public void Configure(EntityTypeBuilder<RefillRecord> builder)
    {
        builder.ToTable("RefillRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 3);
        builder.Property(x => x.UnitCost).HasPrecision(18, 2);
        builder.Property(x => x.TotalCost).HasPrecision(18, 2);
        builder.Property(x => x.SupplierName).HasMaxLength(200);
        builder.Property(x => x.InvoiceNumber).HasMaxLength(50);
        builder.HasOne(x => x.InventoryItem).WithMany(i => i.RefillRecords).HasForeignKey(x => x.InventoryItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.RecordedBy).WithMany().HasForeignKey(x => x.RecordedById).OnDelete(DeleteBehavior.Restrict);
    }
}
