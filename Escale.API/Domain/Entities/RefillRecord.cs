namespace Escale.API.Domain.Entities;

public class RefillRecord : BaseEntity
{
    public Guid InventoryItemId { get; set; }
    public InventoryItem InventoryItem { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? SupplierName { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime RefillDate { get; set; }
    public Guid RecordedById { get; set; }
    public User RecordedBy { get; set; } = null!;
}
