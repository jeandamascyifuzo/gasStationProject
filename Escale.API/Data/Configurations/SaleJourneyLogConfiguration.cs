using Escale.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Escale.API.Data.Configurations;

public class SaleJourneyLogConfiguration : IEntityTypeConfiguration<SaleJourneyLog>
{
    public void Configure(EntityTypeBuilder<SaleJourneyLog> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Liters).HasPrecision(18, 3);
        builder.Property(e => e.PricePerLiter).HasPrecision(18, 2);
        builder.Property(e => e.Total).HasPrecision(18, 2);
        builder.Property(e => e.CashierName).HasMaxLength(200);
        builder.Property(e => e.FuelType).HasMaxLength(100);
        builder.Property(e => e.PaymentMethod).HasMaxLength(50);
        builder.Property(e => e.FailureStep).HasMaxLength(100);
        builder.Property(e => e.FailureReason).HasMaxLength(500);
        builder.Property(e => e.EBMError).HasMaxLength(500);
        builder.Property(e => e.SubscriptionFailReason).HasMaxLength(300);

        builder.HasIndex(e => e.OrganizationId);
        builder.HasIndex(e => e.CashierId);
        builder.HasIndex(e => e.Timestamp);
    }
}
