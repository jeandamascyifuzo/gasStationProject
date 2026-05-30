namespace Escale.API.DTOs.Stock;

public class AdjustEBMStockRequest
{
    public Guid InventoryItemId { get; set; }
    public decimal TargetStock { get; set; }
}

public class StockLevelDto
{
    public Guid Id { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal CurrentLevel { get; set; }
    public decimal Capacity { get; set; }
    public decimal PercentageFull { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public decimal ReorderLevel { get; set; }
}
