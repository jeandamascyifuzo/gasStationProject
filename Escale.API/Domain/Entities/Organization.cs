namespace Escale.API.Domain.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? TIN { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Station> Stations { get; set; } = new List<Station>();
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<FuelType> FuelTypes { get; set; } = new List<FuelType>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public OrganizationSettings? Settings { get; set; }
}
