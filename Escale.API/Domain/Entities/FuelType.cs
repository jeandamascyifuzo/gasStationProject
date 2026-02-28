namespace Escale.API.Domain.Entities;

public class FuelType : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<FuelPrice> PriceHistory { get; set; } = new List<FuelPrice>();
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
