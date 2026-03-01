namespace Escale.API.DTOs.Transactions;

public class TransactionResponseDto
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal Liters { get; set; }
    public decimal PricePerLiter { get; set; }
    public decimal Subtotal { get; set; }
    public decimal VAT { get; set; }
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public bool EBMSent { get; set; }
    public string? EBMReceiptUrl { get; set; }
    public DateTime? EBMSentAt { get; set; }
    public string? EBMErrorMessage { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public string StationName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? SubscriptionDeduction { get; set; }
}

public class TransactionFilterDto
{
    public Guid? StationId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? FuelTypeId { get; set; }
    public string? PaymentMethod { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
