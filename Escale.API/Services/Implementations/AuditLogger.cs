using System.Text.Json;
using Escale.API.Data;
using Escale.API.Domain.Entities;
using Escale.API.Services.Interfaces;

namespace Escale.API.Services.Implementations;

public class AuditLogger : IAuditLogger
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogger(IServiceScopeFactory scopeFactory, ICurrentUserService currentUser, IHttpContextAccessor httpContextAccessor)
    {
        _scopeFactory = scopeFactory;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string action, string entityType, string? entityId, object? details = null)
    {
        await LogAsync(
            _currentUser.OrganizationId,
            _currentUser.UserId,
            _currentUser.Username,
            action, entityType, entityId, details);
    }

    public async Task LogAsync(Guid? orgId, Guid? userId, string? userName, string action, string entityType, string? entityId, object? details = null)
    {
        try
        {
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            string? detailsJson = null;
            if (details != null)
            {
                detailsJson = details is string s ? s : JsonSerializer.Serialize(details);
            }

            // Use a separate DbContext scope to avoid interfering with the business transaction
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EscaleDbContext>();

            var auditLog = new AuditLog
            {
                OrganizationId = orgId,
                UserId = userId,
                UserName = userName,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = detailsJson,
                IpAddress = ip,
                Timestamp = DateTime.UtcNow
            };
            context.AuditLogs.Add(auditLog);
            await context.SaveChangesAsync();
            Console.WriteLine($"[AuditLogger OK] {action} {entityType} OrgId={orgId}");
        }
        catch (Exception ex)
        {
            // Log but never let audit logging break business operations
            Console.WriteLine($"[AuditLogger ERROR] {action} {entityType}: {ex.Message}");
        }
    }
}
