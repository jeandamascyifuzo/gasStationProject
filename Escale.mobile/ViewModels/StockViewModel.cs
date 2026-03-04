using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escale.mobile.Models;
using Escale.mobile.Services;
using System.Collections.ObjectModel;

namespace Escale.mobile.ViewModels;

public partial class StockViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly SignalRService _signalRService;
    private Action<string>? _dataChangedHandler;

    [ObservableProperty]
    private ObservableCollection<StockLevel> stockLevels = new();

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private DateTime lastUpdated;

    public StockViewModel(ApiService apiService, SignalRService signalRService)
    {
        _apiService = apiService;
        _signalRService = signalRService;
    }

    public void SubscribeToNotifications()
    {
        UnsubscribeFromNotifications();
        _dataChangedHandler = changeType =>
        {
            if (changeType == NotificationConstants.SaleCompleted ||
                changeType == NotificationConstants.InventoryChanged)
            {
                MainThread.BeginInvokeOnMainThread(() => RefreshCommand.Execute(null));
            }
        };
        _signalRService.OnDataChanged += _dataChangedHandler;
    }

    public void UnsubscribeFromNotifications()
    {
        if (_dataChangedHandler != null)
        {
            _signalRService.OnDataChanged -= _dataChangedHandler;
            _dataChangedHandler = null;
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        if (!IsRefreshing)
            IsLoading = true;

        try
        {
            var stationId = AppState.Instance.SelectedStation?.Id ?? Guid.Empty;
            var levels = await _apiService.GetStockLevelsAsync(stationId);

            StockLevels.Clear();
            foreach (var level in levels)
            {
                StockLevels.Add(level);
            }

            LastUpdated = DateTime.Now;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Refresh stock error: {ex}");
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load stock: {ex.Message}", "OK");
            }
        }
        finally
        {
            IsRefreshing = false;
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RequestReorder()
    {
        try
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Coming Soon", "Reorder request feature will be available soon", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Request reorder error: {ex}");
        }
    }

    [RelayCommand]
    private async Task ViewHistory()
    {
        try
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Coming Soon", "Stock history feature will be available soon", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"View history error: {ex}");
        }
    }
}
