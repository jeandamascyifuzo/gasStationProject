using Escale.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Escale.API.Data.Configurations;

public class StationConfiguration : IEntityTypeConfiguration<Station>
{
    public void Configure(EntityTypeBuilder<Station> builder)
    {
        builder.ToTable("Stations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Location).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.HasIndex(x => new { x.OrganizationId, x.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasOne(x => x.Organization).WithMany(o => o.Stations).HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Manager).WithMany(u => u.ManagedStations).HasForeignKey(x => x.ManagerId).OnDelete(DeleteBehavior.SetNull);
    }
}
