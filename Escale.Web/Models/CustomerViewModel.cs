using System.ComponentModel.DataAnnotations;

namespace Escale.Web.Models
{
    public class Customer
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Phone]
        public string Phone { get; set; } = string.Empty;
        
        public string? Address { get; set; }
        
        public string? CompanyName { get; set; }
        
        public string? TIN { get; set; }
        
        public string CustomerType { get; set; } = "Individual";
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
        
        public List<Car> Cars { get; set; } = new();
        
        public List<Subscription> Subscriptions { get; set; } = new();
    }

    public class Car
    {
        public int Id { get; set; }
        
        public int CustomerId { get; set; }
        
        [Required]
        public string PlateNumber { get; set; } = string.Empty;
        
        [Required]
        public string CarPin { get; set; } = string.Empty;
        
        [Required]
        public string Model { get; set; } = string.Empty;
        
        public string? Make { get; set; }
        
        public int? Year { get; set; }
        
        public string? Color { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
    }

    public class Subscription
    {
        public int Id { get; set; }
        
        public int CustomerId { get; set; }
        
        [Required]
        public string SubscriptionType { get; set; } = string.Empty;
        
        [Required]
        public decimal Amount { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime? ActivatedDate { get; set; }
        
        public DateTime? ExpiryDate { get; set; }
        
        public string? FuelType { get; set; }
        
        public decimal? MonthlyLimit { get; set; }
        
        public decimal? CurrentUsage { get; set; }
        
        public string? PaymentFrequency { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
    }

    public class CustomerViewModel
    {
        public List<Customer> Customers { get; set; } = new();
        public List<string> CustomerTypes { get; set; } = new() { "Individual", "Corporate" };
    }

    public class CustomerDetailsViewModel
    {
        public Customer Customer { get; set; } = new();
        public List<Car> Cars { get; set; } = new();
        public List<Subscription> Subscriptions { get; set; } = new();
        public CustomerStats Stats { get; set; } = new();
    }

    public class CustomerStats
    {
        public int TotalCars { get; set; }
        public int ActiveSubscriptions { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastTransaction { get; set; }
    }
}
