namespace Escale.API.Services.Interfaces;

public interface INotificationService
{
    Task NotifyDataChangedAsync(Guid orgId, string changeType);
}
