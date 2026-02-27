namespace Escale.Web.Models.Api;

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

public class StockRefillRequest
{
    public Guid StationId { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? SupplierName { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime RefillDate { get; set; }
}
