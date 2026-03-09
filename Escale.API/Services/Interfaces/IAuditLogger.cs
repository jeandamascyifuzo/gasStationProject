namespace Escale.API.Services.Interfaces;

public interface IAuditLogger
{
    Task LogAsync(string action, string entityType, string? entityId, object? details = null);
    Task LogAsync(Guid? orgId, Guid? userId, string? userName, string action, string entityType, string? entityId, object? details = null);
}
