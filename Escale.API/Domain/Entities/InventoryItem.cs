namespace Escale.API.Domain.Entities;

public class InventoryItem : TenantEntity
{
    public Guid StationId { get; set; }
    public Station Station { get; set; } = null!;
    public Guid FuelTypeId { get; set; }
    public FuelType FuelType { get; set; } = null!;
    public decimal CurrentLevel { get; set; }
    public decimal Capacity { get; set; }
    public decimal ReorderLevel { get; set; }
    public DateTime? LastRefillDate { get; set; }
    public DateTime? NextDeliveryDate { get; set; }

    public ICollection<RefillRecord> RefillRecords { get; set; } = new List<RefillRecord>();
}
