namespace Escale.mobile.Models;

public class SaleModel
{
    public int Id { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal PricePerLiter { get; set; }
    public decimal? AmountRWF { get; set; }
    public decimal? Liters { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public CustomerInfo? Customer { get; set; }
    public decimal Subtotal => (Liters ?? 0) * PricePerLiter;
    public decimal VAT => Subtotal * 0.18m;
    public decimal Total => Subtotal + VAT;
    public DateTime TransactionDate { get; set; } = DateTime.Now;
    public string? EBMCode { get; set; }
    public string? ReceiptNumber { get; set; }
}

public class FuelTypeOption
{
    public string Name { get; set; } = string.Empty;
    public decimal PricePerLiter { get; set; }
    public string Icon { get; set; } = string.Empty;
    public Color BadgeColor { get; set; } = Colors.Green;
    public string DisplayText => $"{Name} - RWF {PricePerLiter:N0}/L";
}

public class CustomerInfo
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? PlateNumber { get; set; }
    public string? CarPIN { get; set; }
    public string? VehicleModel { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal? CurrentCredit { get; set; }
    public bool IsWalkIn => !Id.HasValue;
}
