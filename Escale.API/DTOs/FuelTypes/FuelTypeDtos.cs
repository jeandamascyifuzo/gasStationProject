namespace Escale.API.DTOs.FuelTypes;

public class FuelTypeResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PricePerLiter { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? EBMVariantId { get; set; }
}

public class CreateFuelTypeRequestDto
{
    public string Name { get; set; } = string.Empty;
    public decimal PricePerLiter { get; set; }
    public string? EBMVariantId { get; set; }
    public decimal? EBMSupplyPrice { get; set; }
}

public class UpdateFuelTypeRequestDto
{
    public string Name { get; set; } = string.Empty;
    public decimal PricePerLiter { get; set; }
    public bool IsActive { get; set; }
    public string? EBMVariantId { get; set; }
    public decimal? EBMSupplyPrice { get; set; }
}
