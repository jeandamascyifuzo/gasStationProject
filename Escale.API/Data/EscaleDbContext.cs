using Escale.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Data;

public class EscaleDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public EscaleDbContext(DbContextOptions<EscaleDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserStation> UserStations => Set<UserStation>();
    public DbSet<FuelType> FuelTypes => Set<FuelType>();
    public DbSet<FuelPrice> FuelPrices => Set<FuelPrice>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<RefillRecord> RefillRecords => Set<RefillRecord>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<OrganizationSettings> OrganizationSettings => Set<OrganizationSettings>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EscaleDbContext).Assembly);

        // Global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(EscaleDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, new object[] { modelBuilder });
            }
        }
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : BaseEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _httpContextAccessor?.HttpContext?.User?.FindFirst("UserId")?.Value;
        var orgId = _httpContextAccessor?.HttpContext?.User?.FindFirst("OrganizationId")?.Value;
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;
            }
        }

        // Auto-set OrganizationId on new TenantEntity records
        // For SuperAdmin, check X-Organization-Id header first
        var effectiveOrgId = orgId;
        var userRole = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (userRole == "SuperAdmin")
        {
            var headerOrgId = _httpContextAccessor?.HttpContext?.Request.Headers["X-Organization-Id"].FirstOrDefault();
            if (!string.IsNullOrEmpty(headerOrgId))
                effectiveOrgId = headerOrgId;
        }

        if (effectiveOrgId != null && Guid.TryParse(effectiveOrgId, out var organizationId))
        {
            foreach (var entry in ChangeTracker.Entries<TenantEntity>())
            {
                if (entry.State == EntityState.Added && entry.Entity.OrganizationId == Guid.Empty)
                {
                    entry.Entity.OrganizationId = organizationId;
                }
            }
        }

        // Soft delete interception
        foreach (var entry in ChangeTracker.Entries<BaseEntity>().Where(e => e.State == EntityState.Deleted))
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = now;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
