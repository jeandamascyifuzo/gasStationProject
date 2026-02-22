using Escale.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Escale.API.Data.Configurations;

public class UserStationConfiguration : IEntityTypeConfiguration<UserStation>
{
    public void Configure(EntityTypeBuilder<UserStation> builder)
    {
        builder.ToTable("UserStations");
        builder.HasKey(x => new { x.UserId, x.StationId });
        builder.HasOne(x => x.User).WithMany(u => u.UserStations).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Station).WithMany(s => s.UserStations).HasForeignKey(x => x.StationId).OnDelete(DeleteBehavior.Cascade);
    }
}
