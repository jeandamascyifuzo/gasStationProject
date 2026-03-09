using System.Text.Json;
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
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

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
        var userName = _httpContextAccessor?.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var ipAddress = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        var now = DateTime.UtcNow;

        // Collect audit entries before modifying states
        var auditEntries = new List<AuditLog>();
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            // Skip AuditLog itself and RefreshToken (noise)
            if (entry.Entity is AuditLog || entry.Entity is RefreshToken)
                continue;

            var entityType = entry.Entity.GetType().Name;
            var entityId = entry.Entity.Id.ToString();

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    entityId = entry.Entity.Id.ToString();
                    auditEntries.Add(BuildAuditLog("Create", entityType, entityId, entry, userId, userName, orgId, ipAddress, now));
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    auditEntries.Add(BuildAuditLog("Update", entityType, entityId, entry, userId, userName, orgId, ipAddress, now));
                    break;
                case EntityState.Deleted:
                    auditEntries.Add(BuildAuditLog("Delete", entityType, entityId, entry, userId, userName, orgId, ipAddress, now));
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

            // Set OrganizationId on audit entries that don't have one
            foreach (var audit in auditEntries.Where(a => a.OrganizationId == null))
            {
                audit.OrganizationId = organizationId;
            }
        }

        // Soft delete interception
        foreach (var entry in ChangeTracker.Entries<BaseEntity>().Where(e => e.State == EntityState.Deleted))
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAt = now;
        }

        // Add audit logs to the context
        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private static AuditLog BuildAuditLog(string action, string entityType, string entityId,
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry,
        string? userId, string? userName, string? orgId, string? ipAddress, DateTime timestamp)
    {
        // Resolve the effective org ID from the entity if it's a TenantEntity
        Guid? auditOrgId = null;
        if (orgId != null && Guid.TryParse(orgId, out var parsedOrgId))
            auditOrgId = parsedOrgId;
        if (entry.Entity is TenantEntity tenant && tenant.OrganizationId != Guid.Empty)
            auditOrgId = tenant.OrganizationId;
        if (entry.Entity is Organization org)
            auditOrgId = org.Id;
        if (entry.Entity is OrganizationSettings settings)
            auditOrgId = settings.OrganizationId;

        string? details = null;
        try
        {
            if (action == "Update")
            {
                var changes = new Dictionary<string, object?>();
                foreach (var prop in entry.Properties.Where(p => p.IsModified))
                {
                    // Skip audit fields and password hashes
                    var name = prop.Metadata.Name;
                    if (name is "UpdatedAt" or "UpdatedBy" or "CreatedAt" or "CreatedBy" or "PasswordHash")
                        continue;
                    changes[name] = new { Old = prop.OriginalValue, New = prop.CurrentValue };
                }
                if (changes.Count > 0)
                    details = JsonSerializer.Serialize(changes);
            }
            else if (action == "Create")
            {
                var props = new Dictionary<string, object?>();
                foreach (var prop in entry.Properties)
                {
                    var name = prop.Metadata.Name;
                    if (name is "PasswordHash" or "CreatedBy" or "UpdatedBy")
                        continue;
                    if (prop.CurrentValue != null)
                        props[name] = prop.CurrentValue;
                }
                details = JsonSerializer.Serialize(props);
            }
            else if (action == "Delete")
            {
                details = JsonSerializer.Serialize(new { EntityType = entityType, EntityId = entityId });
            }
        }
        catch
        {
            // Don't let audit serialization failures break the save
        }

        return new AuditLog
        {
            OrganizationId = auditOrgId,
            UserId = userId != null && Guid.TryParse(userId, out var uid) ? uid : null,
            UserName = userName,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress,
            Timestamp = timestamp
        };
    }
}
