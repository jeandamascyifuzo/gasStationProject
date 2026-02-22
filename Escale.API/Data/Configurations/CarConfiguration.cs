using Escale.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Escale.API.Data.Configurations;

public class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.ToTable("Cars");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PlateNumber).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Make).HasMaxLength(100);
        builder.Property(x => x.Model).HasMaxLength(100);
        builder.HasOne(x => x.Customer).WithMany(c => c.Cars).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
    }
}
