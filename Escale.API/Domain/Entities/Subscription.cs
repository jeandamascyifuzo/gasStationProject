using Escale.API.Domain.Enums;

namespace Escale.API.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid FuelTypeId { get; set; }
    public FuelType FuelType { get; set; } = null!;
    public decimal MonthlyLiters { get; set; }
    public decimal UsedLiters { get; set; }
    public decimal PricePerLiter { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
}
