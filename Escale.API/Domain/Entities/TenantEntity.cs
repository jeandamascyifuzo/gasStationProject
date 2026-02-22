namespace Escale.API.Domain.Entities;

public abstract class TenantEntity : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
}
