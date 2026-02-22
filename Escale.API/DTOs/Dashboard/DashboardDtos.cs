namespace Escale.API.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public decimal TodaysSales { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageSale { get; set; }
    public List<StockAlertDto> LowStockAlerts { get; set; } = new();
    public List<RecentTransactionDto> RecentTransactions { get; set; } = new();
}

public class StockAlertDto
{
    public string FuelType { get; set; } = string.Empty;
    public decimal CurrentLevel { get; set; }
    public decimal Capacity { get; set; }
    public decimal PercentageFull { get; set; }
}

public class RecentTransactionDto
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal Liters { get; set; }
    public decimal Total { get; set; }
}
