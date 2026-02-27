namespace Escale.Web.Models.Api;

public class SalesReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalSales { get; set; }
    public int TransactionCount { get; set; }
    public List<SalesByFuelTypeDto> SalesByFuel { get; set; } = new();
    public List<SalesByPaymentMethodDto> SalesByPayment { get; set; } = new();
    public List<DailySalesDto> DailySales { get; set; } = new();
}

public class SalesByFuelTypeDto
{
    public string FuelType { get; set; } = string.Empty;
    public decimal Liters { get; set; }
    public decimal Amount { get; set; }
    public int TransactionCount { get; set; }
}

public class SalesByPaymentMethodDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int TransactionCount { get; set; }
}

public class DailySalesDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public int TransactionCount { get; set; }
}

public class InventoryReportDto
{
    public string StationName { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public decimal CurrentLevel { get; set; }
    public decimal Capacity { get; set; }
    public decimal PercentageFull { get; set; }
    public string Status { get; set; } = string.Empty;
    public int RefillCount { get; set; }
    public decimal TotalRefillCost { get; set; }
}

public class EmployeeReportDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalSales { get; set; }
    public int ShiftCount { get; set; }
    public double TotalHoursWorked { get; set; }
}

public class CustomerReportDto
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal TotalLiters { get; set; }
    public decimal CurrentCredit { get; set; }
}

public class FinancialReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalVAT { get; set; }
    public decimal TotalRefillCost { get; set; }
    public decimal GrossProfit { get; set; }
    public List<DailySalesDto> DailyRevenue { get; set; } = new();
}
