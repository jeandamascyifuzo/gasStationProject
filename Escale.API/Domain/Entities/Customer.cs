using Escale.API.Domain.Enums;

namespace Escale.API.Domain.Entities;

public class Customer : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public CustomerType Type { get; set; } = CustomerType.Individual;
    public string? TIN { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentCredit { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Car> Cars { get; set; } = new List<Car>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
