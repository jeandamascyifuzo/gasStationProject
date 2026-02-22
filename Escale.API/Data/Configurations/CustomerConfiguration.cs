using Escale.API.Domain.Entities;
using Escale.API.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Escale.API.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.TIN).HasMaxLength(50);
        builder.Property(x => x.CreditLimit).HasPrecision(18, 2);
        builder.Property(x => x.CurrentCredit).HasPrecision(18, 2);
        builder.HasOne(x => x.Organization).WithMany(o => o.Customers).HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
    }
}
