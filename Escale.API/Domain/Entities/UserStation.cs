namespace Escale.API.Domain.Entities;

public class UserStation
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid StationId { get; set; }
    public Station Station { get; set; } = null!;
    public DateTime AssignedAt { get; set; }
}
