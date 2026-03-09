namespace Escale.API.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? OrganizationId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty; // Login, Logout, Create, Update, Delete, Sale, Refill, etc.
    public string? EntityType { get; set; } // FuelType, Transaction, Station, User, etc.
    public string? EntityId { get; set; }
    public string? Details { get; set; } // JSON with relevant change details
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
