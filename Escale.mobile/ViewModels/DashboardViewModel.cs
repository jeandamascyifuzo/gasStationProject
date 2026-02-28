using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escale.mobile.Models;
using Escale.mobile.Services;
using System.Collections.ObjectModel;

namespace Escale.mobile.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    private string stationName = string.Empty;

    [ObservableProperty]
    private decimal todaysSales;

    [ObservableProperty]
    private int transactionCount;

    [ObservableProperty]
    private decimal averageSale;

    [ObservableProperty]
    private List<StockAlert> lowStockAlerts = new();

    [ObservableProperty]
    private ObservableCollection<RecentTransaction> recentTransactions = new();

    [ObservableProperty]
    private bool hasLowStock;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool isLoading = true;

    [ObservableProperty]
    private bool hasData;

    public DashboardViewModel(ApiService apiService)
    {
        _apiService = apiService;
        LoadDashboard();
    }

    private async void LoadDashboard()
    {
        UserName = AppState.Instance.CurrentUser?.FullName ?? "User";
        StationName = AppState.Instance.SelectedStation?.Name ?? "Station";

        await LoadFromApi();
    }

    private async Task LoadFromApi()
    {
        try
        {
            if (!IsRefreshing)
                IsLoading = true;

            var stationId = AppState.Instance.SelectedStation?.Id;
            var summary = await _apiService.GetDashboardSummaryAsync(stationId);

            TodaysSales = summary.TodaysSales;
            TransactionCount = summary.TransactionCount;
            AverageSale = summary.AverageSale;

            LowStockAlerts = summary.LowStockAlerts.Select(a => new StockAlert
            {
                FuelType = a.FuelType,
                CurrentLevel = a.CurrentLevel,
                Capacity = a.Capacity
            }).ToList();

            HasLowStock = LowStockAlerts.Count > 0;

            RecentTransactions.Clear();
            foreach (var t in summary.RecentTransactions)
            {
                RecentTransactions.Add(new RecentTransaction
                {
                    FuelType = t.FuelType,
                    TransactionDate = t.TransactionDate,
                    Total = t.Total,
                    Liters = t.Liters
                });
            }

            HasData = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading dashboard: {ex}");
            TodaysSales = 0;
            TransactionCount = 0;
            AverageSale = 0;
            LowStockAlerts = new List<StockAlert>();
            HasLowStock = false;
            RecentTransactions.Clear();
            HasData = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        IsRefreshing = true;
        try
        {
            UserName = AppState.Instance.CurrentUser?.FullName ?? "User";
            StationName = AppState.Instance.SelectedStation?.Name ?? "Station";
            await LoadFromApi();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Refresh error: {ex}");
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to refresh: {ex.Message}", "OK");
            }
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task NewSale()
    {
        try
        {
            AppState.Instance.StartNewSale();
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("///NewSale");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex}");
        }
    }

    [RelayCommand]
    private async Task CheckStock()
    {
        try
        {
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("///Stock");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex}");
        }
    }

    [RelayCommand]
    private async Task ViewTransactions()
    {
        try
        {
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("///Transactions");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation error: {ex}");
        }
    }
}
