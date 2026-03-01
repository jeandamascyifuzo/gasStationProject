using Escale.API.Domain.Entities;
using Escale.API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Escale.API.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReceiptNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Liters).HasPrecision(18, 3);
        builder.Property(x => x.PricePerLiter).HasPrecision(18, 2);
        builder.Property(x => x.Subtotal).HasPrecision(18, 2);
        builder.Property(x => x.VATAmount).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);
        builder.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.CustomerName).HasMaxLength(200);
        builder.Property(x => x.CustomerPhone).HasMaxLength(20);
        builder.Property(x => x.EBMCode).HasMaxLength(500);
        builder.Property(x => x.EBMErrorMessage).HasMaxLength(1000);
        builder.HasIndex(x => x.ReceiptNumber).IsUnique();
        builder.HasIndex(x => x.TransactionDate);
        builder.HasOne(x => x.Organization).WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Station).WithMany(s => s.Transactions).HasForeignKey(x => x.StationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.FuelType).WithMany(f => f.Transactions).HasForeignKey(x => x.FuelTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Customer).WithMany(c => c.Transactions).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Cashier).WithMany(u => u.ProcessedTransactions).HasForeignKey(x => x.CashierId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Shift).WithMany(s => s.Transactions).HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.SetNull);
        builder.Property(x => x.SubscriptionDeduction).HasPrecision(18, 2);
        builder.HasOne(x => x.Subscription).WithMany(s => s.Transactions).HasForeignKey(x => x.SubscriptionId).OnDelete(DeleteBehavior.SetNull);
    }
}
