namespace Escale.API.Domain.Entities;

public class FuelPrice : BaseEntity
{
    public Guid FuelTypeId { get; set; }
    public FuelType FuelType { get; set; } = null!;
    public decimal Price { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
