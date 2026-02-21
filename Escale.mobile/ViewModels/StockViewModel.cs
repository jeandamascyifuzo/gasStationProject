using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escale.mobile.Models;
using Escale.mobile.Services;
using System.Collections.ObjectModel;

namespace Escale.mobile.ViewModels;

public partial class StockViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    
    [ObservableProperty]
    private ObservableCollection<StockLevel> stockLevels = new();
    
    [ObservableProperty]
    private bool isRefreshing;
    
    [ObservableProperty]
    private DateTime lastUpdated;

    public StockViewModel(ApiService apiService)
    {
        _apiService = apiService;
        LoadStock();
    }

    private async void LoadStock()
    {
        await Refresh();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        IsRefreshing = true;
        try
        {
            // Simulate API delay
            await Task.Delay(800);
            
            // Load dummy stock data
            var levels = GetDummyStockLevels();
            
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
        }
    }

    private List<StockLevel> GetDummyStockLevels()
    {
        try
        {
            var random = new Random();
            return new List<StockLevel>
            {
                new StockLevel
                {
                    FuelType = "Petrol 95",
                    CurrentLevel = random.Next(8000, 12000),
                    Capacity = 15000,
                    LastUpdated = DateTime.Now
                },
                new StockLevel
                {
                    FuelType = "Petrol 98",
                    CurrentLevel = random.Next(6000, 9000),
                    Capacity = 12000,
                    LastUpdated = DateTime.Now
                },
                new StockLevel
                {
                    FuelType = "Diesel",
                    CurrentLevel = random.Next(2000, 3500),
                    Capacity = 20000,
                    LastUpdated = DateTime.Now
                },
                new StockLevel
                {
                    FuelType = "Kerosene",
                    CurrentLevel = random.Next(3500, 5000),
                    Capacity = 10000,
                    LastUpdated = DateTime.Now
                }
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error generating dummy stock: {ex}");
            return new List<StockLevel>();
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
