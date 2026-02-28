namespace Escale.API.DTOs.Subscriptions;

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

public class LookupCarRequestDto
{
    public string PlateNumber { get; set; } = string.Empty;
    public string PIN { get; set; } = string.Empty;
    public decimal? SaleAmount { get; set; }
}

public class SubscriptionCustomerLookupResponseDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool CustomerIsActive { get; set; }
    public Guid CarId { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string? CarMake { get; set; }
    public string? CarModel { get; set; }
    public bool CarIsActive { get; set; }
    public Guid? ActiveSubscriptionId { get; set; }
    public decimal? RemainingBalance { get; set; }
    public DateTime? SubscriptionExpiryDate { get; set; }
    public bool HasSufficientBalance { get; set; }
    public string? ValidationError { get; set; }
}
