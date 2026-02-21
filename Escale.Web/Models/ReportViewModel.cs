using System.ComponentModel.DataAnnotations;

namespace Escale.Web.Models
{
    public class ReportViewModel
    {
        public string ReportType { get; set; } = "Sales";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? StationId { get; set; }
        public int? FuelTypeId { get; set; }
        
        public List<Station> Stations { get; set; } = new();
        public List<FuelType> FuelTypes { get; set; } = new();
    }

    public class SalesReportData
    {
        public string Station { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal TotalSales { get; set; }
        public int TransactionCount { get; set; }
        public decimal AveragePerTransaction { get; set; }
        public DateTime Date { get; set; }
    }

    public class InventoryReportData
    {
        public string Station { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public decimal OpeningStock { get; set; }
        public decimal Received { get; set; }
        public decimal Sold { get; set; }
        public decimal ClosingStock { get; set; }
        public decimal Variance { get; set; }
    }

    public class EmployeeReportData
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Station { get; set; } = string.Empty;
        public int TransactionsProcessed { get; set; }
        public decimal TotalSales { get; set; }
        public DateTime Date { get; set; }
    }

    public class CustomerReportData
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerType { get; set; } = string.Empty;
        public int TotalCars { get; set; }
        public int ActiveSubscriptions { get; set; }
        public decimal TotalSpent { get; set; }
        public int TransactionCount { get; set; }
        public DateTime LastTransaction { get; set; }
    }

    public class FinancialReportData
    {
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CostOfGoods { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal Expenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
    }
}
