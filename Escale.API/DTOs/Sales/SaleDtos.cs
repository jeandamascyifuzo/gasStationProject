namespace Escale.API.DTOs.Sales;

public class CreateSaleRequestDto
{
    public Guid StationId { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public Guid? FuelTypeId { get; set; }
    public decimal Liters { get; set; }
    public decimal PricePerLiter { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public SaleCustomerDto? Customer { get; set; }
}

public class SaleCustomerDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? PlateNumber { get; set; }
}

public class SaleResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public CompletedSaleDto? Sale { get; set; }
}

public class CompletedSaleDto
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string EBMCode { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal Liters { get; set; }
    public decimal PricePerLiter { get; set; }
    public decimal Subtotal { get; set; }
    public decimal VAT { get; set; }
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
}
