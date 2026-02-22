using Escale.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Escale.API.Data.Configurations;

public class FuelPriceConfiguration : IEntityTypeConfiguration<FuelPrice>
{
    public void Configure(EntityTypeBuilder<FuelPrice> builder)
    {
        builder.ToTable("FuelPrices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.HasOne(x => x.FuelType).WithMany(f => f.PriceHistory).HasForeignKey(x => x.FuelTypeId).OnDelete(DeleteBehavior.Cascade);
    }
}
