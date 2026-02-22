using Escale.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Escale.API.Data.Configurations;

public class FuelTypeConfiguration : IEntityTypeConfiguration<FuelType>
{
    public void Configure(EntityTypeBuilder<FuelType> builder)
    {
        builder.ToTable("FuelTypes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CurrentPrice).HasPrecision(18, 2);
        builder.HasIndex(x => new { x.OrganizationId, x.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasOne(x => x.Organization).WithMany(o => o.FuelTypes).HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
    }
}
