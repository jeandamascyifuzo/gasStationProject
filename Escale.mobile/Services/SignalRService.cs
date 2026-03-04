using Microsoft.AspNetCore.SignalR.Client;

namespace Escale.mobile.Services;

public class SignalRService
{
    private HubConnection? _connection;
    private readonly object _lock = new();
    private bool _isConnecting;

    public event Action<string>? OnDataChanged;
    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public async Task ConnectAsync(string token)
    {
        lock (_lock)
        {
            if (_isConnecting || IsConnected) return;
            _isConnecting = true;
        }

        try
        {
            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }

            var baseUrl = ApiService.GetBaseUrl().Replace("/api", "");
            var hubUrl = $"{baseUrl}/hubs/escale";

            var builder = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(token);
#if DEBUG
                    options.HttpMessageHandlerFactory = _ => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                    };
                    options.WebSocketConfiguration = socket =>
                    {
                        socket.RemoteCertificateValidationCallback = (_, _, _, _) => true;
                    };
#endif
                })
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) });

            _connection = builder.Build();

            _connection.On<string>("DataChanged", changeType =>
            {
                System.Diagnostics.Debug.WriteLine($"[SignalR] DataChanged: {changeType}");
                OnDataChanged?.Invoke(changeType);
            });

            _connection.Reconnecting += error =>
            {
                System.Diagnostics.Debug.WriteLine($"[SignalR] Reconnecting: {error?.Message}");
                return Task.CompletedTask;
            };

            _connection.Reconnected += connectionId =>
            {
                System.Diagnostics.Debug.WriteLine($"[SignalR] Reconnected: {connectionId}");
                return Task.CompletedTask;
            };

            _connection.Closed += error =>
            {
                System.Diagnostics.Debug.WriteLine($"[SignalR] Connection closed: {error?.Message}");
                return Task.CompletedTask;
            };

            await _connection.StartAsync();
            System.Diagnostics.Debug.WriteLine("[SignalR] Connected successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SignalR] Connection failed: {ex.Message}");
        }
        finally
        {
            lock (_lock)
            {
                _isConnecting = false;
            }
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            if (_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
                _connection = null;
                System.Diagnostics.Debug.WriteLine("[SignalR] Disconnected");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SignalR] Disconnect error: {ex.Message}");
        }
    }
}
