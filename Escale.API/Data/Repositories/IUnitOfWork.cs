using Escale.API.Domain.Entities;

namespace Escale.API.Data.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<Organization> Organizations { get; }
    IRepository<Station> Stations { get; }
    IRepository<User> Users { get; }
    IRepository<FuelType> FuelTypes { get; }
    IRepository<FuelPrice> FuelPrices { get; }
    IRepository<Customer> Customers { get; }
    IRepository<Car> Cars { get; }
    IRepository<Subscription> Subscriptions { get; }
    IRepository<Transaction> Transactions { get; }
    IRepository<InventoryItem> InventoryItems { get; }
    IRepository<RefillRecord> RefillRecords { get; }
    IRepository<Shift> Shifts { get; }
    IRepository<OrganizationSettings> OrganizationSettings { get; }
    IRepository<RefreshToken> RefreshTokens { get; }
    EscaleDbContext Context { get; }
    Task<int> SaveChangesAsync();
}
