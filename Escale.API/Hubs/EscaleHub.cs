using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Escale.API.Hubs;

[Authorize]
public class EscaleHub : Hub
{
    private readonly ILogger<EscaleHub> _logger;

    public EscaleHub(ILogger<EscaleHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var orgId = Context.User?.FindFirst("OrganizationId")?.Value;
        if (!string.IsNullOrEmpty(orgId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"org_{orgId}");
            _logger.LogInformation("Client {ConnectionId} joined group org_{OrgId}", Context.ConnectionId, orgId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var orgId = Context.User?.FindFirst("OrganizationId")?.Value;
        if (!string.IsNullOrEmpty(orgId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"org_{orgId}");
            _logger.LogInformation("Client {ConnectionId} left group org_{OrgId}", Context.ConnectionId, orgId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}
