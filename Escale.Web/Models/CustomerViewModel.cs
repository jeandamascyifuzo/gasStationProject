using System.ComponentModel.DataAnnotations;

namespace Escale.Web.Models
{
    public class Customer
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? TIN { get; set; }

        public string Type { get; set; } = "Individual";

        public decimal CreditLimit { get; set; }
        public decimal CurrentCredit { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<Car> Cars { get; set; } = new();

        public List<Subscription> Subscriptions { get; set; } = new();
    }

    public class Car
    {
        public Guid? Id { get; set; }

        [Required]
        public string PlateNumber { get; set; } = string.Empty;

        public string? Make { get; set; }

        public string? Model { get; set; }

        public int? Year { get; set; }
    }

    public class Subscription
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

    public class CustomerViewModel
    {
        public List<Customer> Customers { get; set; } = new();
        public List<string> CustomerTypes { get; set; } = new() { "Individual", "Corporate" };
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class CustomerDetailsViewModel
    {
        public Customer Customer { get; set; } = new();
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
