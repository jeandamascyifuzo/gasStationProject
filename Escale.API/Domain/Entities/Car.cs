namespace Escale.API.Domain.Entities;

public class Car : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string PlateNumber { get; set; } = string.Empty;
    public string? Make { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
}
