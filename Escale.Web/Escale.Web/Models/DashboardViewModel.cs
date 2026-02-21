namespace Escale.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalStations { get; set; }
        public int ActivePumps { get; set; }
        public decimal TodaysSales { get; set; }
        public int LowStockAlerts { get; set; }
        public List<DailySales> SalesChart { get; set; } = new();
        public List<FuelSalesData> FuelTypeChart { get; set; } = new();
        public List<StationSalesData> TopStations { get; set; } = new();
        public List<RecentTransaction> RecentTransactions { get; set; } = new();
    }

    public class DailySales
    {
        public string Date { get; set; } = string.Empty;
        public decimal Sales { get; set; }
    }

    public class FuelSalesData
    {
        public string FuelType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class StationSalesData
    {
        public string StationName { get; set; } = string.Empty;
        public decimal Sales { get; set; }
    }

    public class RecentTransaction
    {
        public string TransactionId { get; set; } = string.Empty;
        public string Station { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Total { get; set; }
        public DateTime Time { get; set; }
    }
}
