namespace Escale.API.Domain.Entities;

public class Station : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? ManagerId { get; set; }
    public User? Manager { get; set; }

    public ICollection<UserStation> UserStations { get; set; } = new List<UserStation>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}
