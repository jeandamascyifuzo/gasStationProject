using Escale.mobile.Models;

namespace Escale.mobile.Services;

public class AppState
{
    private static AppState? _instance;
    public static AppState Instance => _instance ?? throw new InvalidOperationException("AppState not initialized. Resolve it from DI first.");

    public UserInfo? CurrentUser { get; set; }
    public StationInfo? SelectedStation { get; set; }   
    public SaleModel? CurrentSale { get; set; }
    public bool IsLoggedIn => CurrentUser != null && !string.IsNullOrEmpty(AuthToken);
    public string? AuthToken { get; set; }
    public DateTime? ShiftStartTime { get; set; }
    public bool IsClockedIn => ShiftStartTime.HasValue;

    public event EventHandler? StateChanged;

    public AppState()
    {
        // Ensure the DI-created instance is the same as the static instance
        _instance = this;
    }

    public void SetUser(UserInfo user, string token)
    {
        CurrentUser = user;
        AuthToken = token;
        OnStateChanged();
    }

    public void SetStation(StationInfo station)
    {
        SelectedStation = station;
        OnStateChanged();
    }

    public void Logout()
    {
        CurrentUser = null;
        SelectedStation = null;
        AuthToken = null;
        CurrentSale = null;
        ShiftStartTime = null;
        OnStateChanged();
    }

    public void StartNewSale()
    {
        CurrentSale = new SaleModel
        {
            TransactionDate = DateTime.Now,
            StationName = SelectedStation?.Name ?? "Unknown Station"
        };
        OnStateChanged();
    }

    public void ClearCurrentSale()
    {
        CurrentSale = null;
        OnStateChanged();
    }

    public void ClockIn()
    {
        ShiftStartTime = DateTime.Now;
        OnStateChanged();
    }

    public void ClockOut()
    {
        ShiftStartTime = null;
        OnStateChanged();
    }

    private void OnStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
};
