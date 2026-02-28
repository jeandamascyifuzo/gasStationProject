using Escale.API.DTOs.Subscriptions;

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
    public List<CarResponseDto> Cars { get; set; } = new();
    public List<SubscriptionResponseDto> Subscriptions { get; set; } = new();
    public SubscriptionResponseDto? ActiveSubscription { get; set; }
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
    public string? PIN { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CarResponseDto
{
    public Guid Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public bool IsActive { get; set; }
}
