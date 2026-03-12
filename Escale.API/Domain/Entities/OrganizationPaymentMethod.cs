namespace Escale.API.Domain.Entities;

public class OrganizationPaymentMethod : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string Name { get; set; } = string.Empty;         // Matches PaymentMethod enum: Cash, MobileMoney, Card, Credit
    public string DisplayName { get; set; } = string.Empty;  // Human-friendly label shown in UI
    public bool IsEnabled { get; set; } = true;
    public int SortOrder { get; set; }
}
