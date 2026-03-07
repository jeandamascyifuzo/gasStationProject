namespace Escale.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalStations { get; set; }
        public int TransactionCount { get; set; }
        public decimal TodaysSales { get; set; }
        public int LowStockAlerts { get; set; }
        public decimal AverageSale { get; set; }
        public decimal CreditSales { get; set; }
        public int CreditTransactionCount { get; set; }
        public List<DailySales> SalesChart { get; set; } = new();
        public List<FuelSalesData> FuelTypeChart { get; set; } = new();
        public List<RecentTransaction> RecentTransactions { get; set; } = new();
        public List<StationPerformance> TopStations { get; set; } = new();
    }

    public class StationPerformance
    {
        public Guid StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
        public int TransactionCount { get; set; }
        public decimal TotalLiters { get; set; }
        public decimal CashSales { get; set; }
        public decimal CreditSales { get; set; }
        public int Rank { get; set; }
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

    public class RecentTransaction
    {
        public string TransactionId { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string CashierName { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}
