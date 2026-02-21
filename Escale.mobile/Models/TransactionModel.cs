namespace Escale.mobile.Models;

public class StockLevel
{
    public string FuelType { get; set; } = string.Empty;
    public decimal CurrentLevel { get; set; }
    public decimal Capacity { get; set; }
    public decimal PercentageRemaining => (CurrentLevel / Capacity) * 100;
    public string Status => PercentageRemaining < 20 ? "Low Stock" : 
                            PercentageRemaining < 50 ? "Medium" : "Good";
    public Color StatusColor => PercentageRemaining < 20 ? Colors.Red :
                                PercentageRemaining < 50 ? Colors.Orange : Colors.Green;
    public DateTime LastUpdated { get; set; }
}

public class Transaction
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal Liters { get; set; }
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? PlateNumber { get; set; }
    public bool EBMSent { get; set; }
    public string? EBMCode { get; set; }
}

public class ShiftSummary
{
    public DateTime ShiftStart { get; set; }
    public DateTime? ShiftEnd { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalSales { get; set; }
    public Dictionary<string, decimal> SalesByPaymentMethod { get; set; } = new();
    public Dictionary<string, decimal> SalesByFuelType { get; set; } = new();
}
