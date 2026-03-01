using Escale.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Escale.API.Data.Configurations;

public class OrganizationSettingsConfiguration : IEntityTypeConfiguration<OrganizationSettings>
{
    public void Configure(EntityTypeBuilder<OrganizationSettings> builder)
    {
        builder.ToTable("OrganizationSettings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CompanyName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TaxRate).HasPrecision(5, 4);
        builder.Property(x => x.Currency).HasMaxLength(10);
        builder.Property(x => x.ReceiptHeader).HasMaxLength(1000);
        builder.Property(x => x.ReceiptFooter).HasMaxLength(1000);
        builder.Property(x => x.EBMServerUrl).HasMaxLength(500);
        builder.Property(x => x.EBMBusinessId).HasMaxLength(100);
        builder.Property(x => x.EBMBranchId).HasMaxLength(100);
        builder.Property(x => x.EBMCompanyName).HasMaxLength(200);
        builder.Property(x => x.EBMCompanyAddress).HasMaxLength(500);
        builder.Property(x => x.EBMCompanyPhone).HasMaxLength(50);
        builder.Property(x => x.EBMCompanyTIN).HasMaxLength(50);
        builder.Property(x => x.EBMCategoryId).HasMaxLength(100);
        builder.Property(x => x.MinimumSaleAmount).HasPrecision(18, 2);
        builder.Property(x => x.MaximumSaleAmount).HasPrecision(18, 2);
        builder.Property(x => x.LowStockThreshold).HasPrecision(5, 4);
        builder.Property(x => x.CriticalStockThreshold).HasPrecision(5, 4);
        builder.HasIndex(x => x.OrganizationId).IsUnique();
        builder.HasOne(x => x.Organization).WithOne(o => o.Settings).HasForeignKey<OrganizationSettings>(x => x.OrganizationId).OnDelete(DeleteBehavior.Cascade);
    }
}
