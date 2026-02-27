using System.ComponentModel.DataAnnotations;

namespace Escale.Web.Models
{
    public class Station
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        public string? Address { get; set; }

        [Phone]
        public string? ContactNumber { get; set; }

        public Guid? ManagerId { get; set; }
        public string? ManagerName { get; set; }

        public bool IsActive { get; set; } = true;
        public string Status => IsActive ? "Active" : "Inactive";

        public DateTime CreatedAt { get; set; }
    }

    public class StationViewModel
    {
        public List<Station> Stations { get; set; } = new();
        public List<User> Managers { get; set; } = new();
    }

    public class StationDetailsViewModel
    {
        public Station Station { get; set; } = new();
        public List<StationStock> Stock { get; set; } = new();
        public List<User> Employees { get; set; } = new();
        public StationStats Stats { get; set; } = new();
    }

    public class StationStock
    {
        public Guid InventoryItemId { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public decimal CurrentLevel { get; set; }
        public decimal Capacity { get; set; }
        public decimal ReorderLevel { get; set; }
        public DateTime? LastRefill { get; set; }
        public decimal PercentageFull { get; set; }
        public bool IsLowStock => PercentageFull < 25;
    }

    public class StationStats
    {
        public decimal TodaysSales { get; set; }
        public int TodaysTransactions { get; set; }
        public int TotalEmployees { get; set; }
        public int LowStockItems { get; set; }
    }
}
