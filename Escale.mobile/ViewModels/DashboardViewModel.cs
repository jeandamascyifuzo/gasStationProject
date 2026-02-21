using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escale.mobile.Models;
using Escale.mobile.Services;

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
    private List<StockAlert> lowStockAlerts = new();
    
    [ObservableProperty]
    private bool isRefreshing;

    public DashboardViewModel(ApiService apiService)
    {
        _apiService = apiService;
        LoadDashboard();
    }

    private void LoadDashboard()
    {
        UserName = AppState.Instance.CurrentUser?.FullName ?? "User";
        StationName = AppState.Instance.SelectedStation?.Name ?? "Station";
        
        // Load dummy data on initialization
        LoadDummyData();
    }

    private void LoadDummyData()
    {
        try
        {
            // Generate random sales data
            var random = new Random();
            TodaysSales = random.Next(500000, 2000000);
            TransactionCount = random.Next(15, 50);
            
            // Create dummy low stock alerts
            LowStockAlerts = new List<StockAlert>
            {
                new StockAlert
                {
                    FuelType = "Diesel",
                    CurrentLevel = 2500,
                    Capacity = 20000
                },
                new StockAlert
                {
                    FuelType = "Petrol 95",
                    CurrentLevel = 3200,
                    Capacity = 15000
                }
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading dummy data: {ex}");
            // Set default values to prevent null references
            TodaysSales = 0;
            TransactionCount = 0;
            LowStockAlerts = new List<StockAlert>();
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        IsRefreshing = true;
        try
        {
            // Simulate API delay
            await Task.Delay(1000);
            
            // Refresh with new dummy data
            LoadDummyData();
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
