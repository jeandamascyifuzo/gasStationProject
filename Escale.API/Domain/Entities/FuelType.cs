namespace Escale.API.Domain.Entities;

public class FuelType : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public string? EBMProductId { get; set; }
    public string? EBMVariantId { get; set; }
    public decimal? EBMSupplyPrice { get; set; }

    public ICollection<FuelPrice> PriceHistory { get; set; } = new List<FuelPrice>();
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
