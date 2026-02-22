namespace Escale.API.Domain.Constants;

public static class BusinessRules
{
    public const decimal VATRate = 0.18m;
    public const decimal LowStockThresholdPercent = 0.25m;
    public const decimal CriticalStockThresholdPercent = 0.10m;
    public const decimal DefaultMinimumSaleAmount = 1000m;
    public const decimal DefaultMaximumSaleAmount = 10_000_000m;
    public const string ReceiptNumberFormat = "RCP{0:yyyyMMddHHmmss}";
    public const string DefaultCurrency = "RWF";
    public const string DefaultReceiptHeader = "ESCALE GAS STATION\nKigali, Rwanda";
    public const string DefaultReceiptFooter = "Thank you for your business!";
}
