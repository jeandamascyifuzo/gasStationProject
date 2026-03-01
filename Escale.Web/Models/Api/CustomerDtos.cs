namespace Escale.Web.Models.Api;

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
    public List<SubscriptionResponseDto> Subscriptions { get; set; } = new();
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

public class SubscriptionResponseDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public decimal PreviousBalance { get; set; }
    public decimal TopUpAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TopUpSubscriptionRequestDto
{
    public Guid CustomerId { get; set; }
    public decimal TopUpAmount { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
