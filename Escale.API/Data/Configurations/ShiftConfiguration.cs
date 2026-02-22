using Escale.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Escale.API.Data.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("Shifts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OpeningCash).HasPrecision(18, 2);
        builder.Property(x => x.ClosingCash).HasPrecision(18, 2);
        builder.HasOne(x => x.Organization).WithMany().HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.User).WithMany(u => u.Shifts).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Station).WithMany(s => s.Shifts).HasForeignKey(x => x.StationId).OnDelete(DeleteBehavior.Restrict);
    }
}
