namespace Escale.Web.Models
{
    public class InventoryItem
    {
        public Guid Id { get; set; }

        public Guid StationId { get; set; }
        public string StationName { get; set; } = string.Empty;

        public Guid FuelTypeId { get; set; }
        public string FuelType { get; set; } = string.Empty;

        public decimal CurrentLevel { get; set; }
        public decimal Capacity { get; set; }
        public decimal ReorderLevel { get; set; }

        public decimal PercentageFull { get; set; }
        public bool IsLowStock => PercentageFull < 25;

        public DateTime? LastRefill { get; set; }

        public DateTime? NextDeliveryDate { get; set; }

        public string Status { get; set; } = "Normal";
    }

    public class RefillRecord
    {
        public Guid Id { get; set; }

        public string StationName { get; set; } = string.Empty;

        public string FuelType { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }

        public DateTime RefillDate { get; set; }

        public string SupplierName { get; set; } = string.Empty;

        public string? InvoiceNumber { get; set; }

        public string RecordedBy { get; set; } = string.Empty;
    }

    public class InventoryViewModel
    {
        public List<InventoryItem> InventoryItems { get; set; } = new();
        public List<RefillRecord> RecentRefills { get; set; } = new();
        public List<Station> Stations { get; set; } = new();
        public List<FuelType> FuelTypes { get; set; } = new();
        public InventoryStats Stats { get; set; } = new();
        public Guid? SelectedStationId { get; set; }
    }

    public class InventoryStats
    {
        public int TotalItems { get; set; }
        public int LowStockItems { get; set; }
        public int CriticalItems { get; set; }
        public decimal TotalCapacity { get; set; }
        public decimal TotalCurrentStock { get; set; }
    }
}
