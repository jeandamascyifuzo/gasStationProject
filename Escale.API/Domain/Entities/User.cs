using Escale.API.Domain.Enums;

namespace Escale.API.Domain.Entities;

public class User : TenantEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<UserStation> UserStations { get; set; } = new List<UserStation>();
    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    public ICollection<Transaction> ProcessedTransactions { get; set; } = new List<Transaction>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Station> ManagedStations { get; set; } = new List<Station>();
}
