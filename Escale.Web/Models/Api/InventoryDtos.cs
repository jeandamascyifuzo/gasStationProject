namespace Escale.Web.Models.Api;

public class InventoryItemResponseDto
{
    public Guid Id { get; set; }
    public string StationName { get; set; } = string.Empty;
    public Guid StationId { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public Guid FuelTypeId { get; set; }
    public decimal CurrentLevel { get; set; }
    public decimal Capacity { get; set; }
    public decimal PercentageFull { get; set; }
    public decimal ReorderLevel { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastRefill { get; set; }
    public DateTime? NextDeliveryDate { get; set; }
}

public class RefillRecordResponseDto
{
    public Guid Id { get; set; }
    public string StationName { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime RefillDate { get; set; }
    public string RecordedBy { get; set; } = string.Empty;
}

public class CreateRefillRequestDto
{
    public Guid InventoryItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? SupplierName { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime RefillDate { get; set; }
}

public class UpdateReorderLevelRequestDto
{
    public Guid Id { get; set; }
    public decimal ReorderLevel { get; set; }
}
