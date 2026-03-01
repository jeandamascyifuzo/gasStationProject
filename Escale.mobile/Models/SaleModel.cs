namespace Escale.mobile.Models;

public class SaleModel
{
    public Guid Id { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public Guid? FuelTypeId { get; set; }
    public decimal PricePerLiter { get; set; }
    public decimal? AmountRWF { get; set; }
    public decimal? Liters { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public CustomerInfo? Customer { get; set; }
    public string StationName { get; set; } = string.Empty;
    public decimal Total => (Liters ?? 0) * PricePerLiter;
    public decimal VAT => Math.Round(Total * 0.18m, 0);
    public decimal Subtotal => Total - VAT;
    public DateTime TransactionDate { get; set; } = DateTime.Now;
    public string? EBMReceiptUrl { get; set; }
    public string? ReceiptNumber { get; set; }
    public Guid? SubscriptionId { get; set; }
    public decimal? SubscriptionDeduction { get; set; }
    public decimal? SubscriptionRemainingBalance { get; set; }
}

public class FuelTypeOption
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PricePerLiter { get; set; }
    public string Icon { get; set; } = string.Empty;
    public Color BadgeColor { get; set; } = Colors.Green;
    public string DisplayText => $"{Name} - RWF {PricePerLiter:N0}/L";
}

public class CustomerInfo
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? PlateNumber { get; set; }
    public string? CarPIN { get; set; }
    public string? VehicleModel { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal? CurrentCredit { get; set; }
    public Guid? CarId { get; set; }
    public Guid? ActiveSubscriptionId { get; set; }
    public decimal? SubscriptionBalance { get; set; }
    public DateTime? SubscriptionExpiryDate { get; set; }
    public bool HasActiveSubscription => ActiveSubscriptionId.HasValue && ActiveSubscriptionId != Guid.Empty;
    public bool IsWalkIn => !Id.HasValue || Id == Guid.Empty;
}
