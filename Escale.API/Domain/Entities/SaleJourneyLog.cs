namespace Escale.API.Domain.Entities;

public class SaleJourneyLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrganizationId { get; set; }
    public Guid StationId { get; set; }
    public Guid CashierId { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public Guid? ShiftId { get; set; }

    // What the cashier entered
    public string FuelType { get; set; } = string.Empty;
    public decimal Liters { get; set; }
    public decimal PricePerLiter { get; set; }
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public bool HasCustomer { get; set; }
    public bool IsSubscriptionSale { get; set; }

    // Step durations (milliseconds)
    public long FuelTypeLookupMs { get; set; }
    public long SubscriptionCheckMs { get; set; }
    public long EBMSubmissionMs { get; set; }
    public long DBSaveMs { get; set; }
    public long TotalDurationMs { get; set; }

    // EBM
    public bool EBMRequired { get; set; }
    public bool EBMSuccess { get; set; }
    public string? EBMError { get; set; }

    // Subscription
    public bool SubscriptionRequired { get; set; }
    public bool? SubscriptionValid { get; set; }
    public string? SubscriptionFailReason { get; set; }

    // Outcome
    public bool Completed { get; set; }
    public string? FailureStep { get; set; } // "FuelType", "Subscription", "EBM", "DBSave"
    public string? FailureReason { get; set; }
    public Guid? TransactionId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
