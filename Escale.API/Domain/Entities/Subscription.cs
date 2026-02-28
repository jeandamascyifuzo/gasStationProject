using Escale.API.Domain.Enums;

namespace Escale.API.Domain.Entities;

public class Subscription : TenantEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public decimal TotalAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public decimal PreviousBalance { get; set; }
    public decimal TopUpAmount { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
