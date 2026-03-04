using Escale.API.Hubs;
using Escale.API.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Escale.API.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly IHubContext<EscaleHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<EscaleHub> hubContext, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyDataChangedAsync(Guid orgId, string changeType)
    {
        try
        {
            await _hubContext.Clients.Group($"org_{orgId}").SendAsync("DataChanged", changeType);
            _logger.LogDebug("Sent {ChangeType} notification to org_{OrgId}", changeType, orgId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send {ChangeType} notification to org_{OrgId}", changeType, orgId);
        }
    }
}
