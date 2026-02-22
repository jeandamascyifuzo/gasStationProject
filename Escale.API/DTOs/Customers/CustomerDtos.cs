using Escale.API.Domain.Enums;

namespace Escale.API.DTOs.Customers;

public class CustomerResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? TIN { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentCredit { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CarDto> Cars { get; set; } = new();
    public List<SubscriptionDto> Subscriptions { get; set; } = new();
}

public class CreateCustomerRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string Type { get; set; } = "Individual";
    public string? TIN { get; set; }
    public decimal CreditLimit { get; set; }
    public List<CarDto>? Cars { get; set; }
}

public class UpdateCustomerRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string Type { get; set; } = "Individual";
    public string? TIN { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
}

public class CarDto
{
    public Guid? Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
}

public class SubscriptionDto
{
    public Guid Id { get; set; }
    public string FuelTypeName { get; set; } = string.Empty;
    public Guid FuelTypeId { get; set; }
    public decimal MonthlyLiters { get; set; }
    public decimal UsedLiters { get; set; }
    public decimal PricePerLiter { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
