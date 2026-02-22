using Escale.API.Domain.Entities;

namespace Escale.API.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly EscaleDbContext _context;
    private IRepository<Organization>? _organizations;
    private IRepository<Station>? _stations;
    private IRepository<User>? _users;
    private IRepository<FuelType>? _fuelTypes;
    private IRepository<FuelPrice>? _fuelPrices;
    private IRepository<Customer>? _customers;
    private IRepository<Car>? _cars;
    private IRepository<Subscription>? _subscriptions;
    private IRepository<Transaction>? _transactions;
    private IRepository<InventoryItem>? _inventoryItems;
    private IRepository<RefillRecord>? _refillRecords;
    private IRepository<Shift>? _shifts;
    private IRepository<OrganizationSettings>? _organizationSettings;
    private IRepository<RefreshToken>? _refreshTokens;

    public UnitOfWork(EscaleDbContext context)
    {
        _context = context;
    }

    public IRepository<Organization> Organizations => _organizations ??= new Repository<Organization>(_context);
    public IRepository<Station> Stations => _stations ??= new Repository<Station>(_context);
    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<FuelType> FuelTypes => _fuelTypes ??= new Repository<FuelType>(_context);
    public IRepository<FuelPrice> FuelPrices => _fuelPrices ??= new Repository<FuelPrice>(_context);
    public IRepository<Customer> Customers => _customers ??= new Repository<Customer>(_context);
    public IRepository<Car> Cars => _cars ??= new Repository<Car>(_context);
    public IRepository<Subscription> Subscriptions => _subscriptions ??= new Repository<Subscription>(_context);
    public IRepository<Transaction> Transactions => _transactions ??= new Repository<Transaction>(_context);
    public IRepository<InventoryItem> InventoryItems => _inventoryItems ??= new Repository<InventoryItem>(_context);
    public IRepository<RefillRecord> RefillRecords => _refillRecords ??= new Repository<RefillRecord>(_context);
    public IRepository<Shift> Shifts => _shifts ??= new Repository<Shift>(_context);
    public IRepository<OrganizationSettings> OrganizationSettings => _organizationSettings ??= new Repository<OrganizationSettings>(_context);
    public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);
    public EscaleDbContext Context => _context;

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}
