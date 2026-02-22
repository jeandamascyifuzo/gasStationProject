namespace Escale.API.Domain.Entities;

public class Shift : TenantEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid StationId { get; set; }
    public Station Station { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal OpeningCash { get; set; }
    public decimal? ClosingCash { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
