using System.ComponentModel.DataAnnotations;

namespace Escale.Web.Models
{
    public class Station
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Location { get; set; } = string.Empty;
        
        public string Address { get; set; } = string.Empty;
        
        [Phone]
        public string ContactNumber { get; set; } = string.Empty;
        
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        
        public string Status { get; set; } = "Active";
        
        public int PumpCount { get; set; }
    }

    public class StationViewModel
    {
        public List<Station> Stations { get; set; } = new();
        public List<User> Managers { get; set; } = new();
    }

    public class StationDetailsViewModel
    {
        public Station Station { get; set; } = new();
        public List<StationSale> Sales { get; set; } = new();
        public List<StationStock> Stock { get; set; } = new();
        public List<User> Employees { get; set; } = new();
        public StationStats Stats { get; set; } = new();
    }

    public class StationSale
    {
        public DateTime Date { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal TotalSales { get; set; }
        public int TransactionCount { get; set; }
    }

    public class StationStock
    {
        public string FuelType { get; set; } = string.Empty;
        public decimal CurrentLevel { get; set; }
        public decimal Capacity { get; set; }
        public decimal ReorderLevel { get; set; }
        public DateTime LastRefill { get; set; }
        public decimal PercentageFull => (CurrentLevel / Capacity) * 100;
        public bool IsLowStock => CurrentLevel <= ReorderLevel;
    }

    public class StationStats
    {
        public decimal TodaysSales { get; set; }
        public int TodaysTransactions { get; set; }
        public int TotalEmployees { get; set; }
        public int ActivePumps { get; set; }
        public int LowStockItems { get; set; }
    }
}
