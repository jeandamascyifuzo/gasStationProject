using Escale.API.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Escale.API.Services.Implementations;

/// <summary>
/// Runs once a day. For each organization that has audit logs older than 6 months,
/// deletes all logs older than 3 months for that org.
/// </summary>
public class AuditLogCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AuditLogCleanupService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    public AuditLogCleanupService(IServiceProvider services, ILogger<AuditLogCleanupService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AuditLogCleanupService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var auditService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
                await auditService.PurgeOldLogsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "AuditLogCleanupService encountered an error.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}
