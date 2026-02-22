using Escale.API.Domain.Entities;
using Escale.API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Escale.API.Data.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MonthlyLiters).HasPrecision(18, 2);
        builder.Property(x => x.UsedLiters).HasPrecision(18, 2);
        builder.Property(x => x.PricePerLiter).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasOne(x => x.Customer).WithMany(c => c.Subscriptions).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.FuelType).WithMany(f => f.Subscriptions).HasForeignKey(x => x.FuelTypeId).OnDelete(DeleteBehavior.Restrict);
    }
}
