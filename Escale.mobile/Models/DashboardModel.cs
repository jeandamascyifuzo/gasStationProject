namespace Escale.mobile.Models;

public class DashboardSummary
{
    public decimal TodaysSales { get; set; }
    public int TransactionCount { get; set; }
    public List<StockAlert> LowStockAlerts { get; set; } = new();
    public string StationName { get; set; } = string.Empty;
    public string CashierName { get; set; } = string.Empty;
}

public class StockAlert
{
    public string FuelType { get; set; } = string.Empty;
    public decimal CurrentLevel { get; set; }
    public decimal Capacity { get; set; }
    public decimal PercentageRemaining => (CurrentLevel / Capacity) * 100;
}
