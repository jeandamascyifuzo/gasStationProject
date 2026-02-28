using Escale.API.Domain.Enums;

namespace Escale.API.Domain.Entities;

public class Transaction : TenantEntity
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public Guid StationId { get; set; }
    public Station Station { get; set; } = null!;
    public Guid FuelTypeId { get; set; }
    public FuelType FuelType { get; set; } = null!;
    public decimal Liters { get; set; }
    public decimal PricePerLiter { get; set; }
    public decimal Subtotal { get; set; }
    public decimal VATAmount { get; set; }
    public decimal Total { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public Guid CashierId { get; set; }
    public User Cashier { get; set; } = null!;
    public Guid? ShiftId { get; set; }
    public Shift? Shift { get; set; }
    public bool EBMSent { get; set; }
    public string? EBMCode { get; set; }
    public Guid? SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    public decimal? SubscriptionDeduction { get; set; }
}
