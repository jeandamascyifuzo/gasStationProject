using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escale.mobile.Models;
using Escale.mobile.Services;

namespace Escale.mobile.ViewModels;

public partial class CustomerInfoViewModel : ObservableObject
{
    [ObservableProperty]
    private string customerName = string.Empty;
    
    [ObservableProperty]
    private string phoneNumber = string.Empty;

    public CustomerInfoViewModel()
    {
    }

    [RelayCommand]
    private async Task Skip()
    {
        await Shell.Current.GoToAsync("../SalePreview");
    }

    [RelayCommand]
    private async Task Continue()
    {
        var sale = AppState.Instance.CurrentSale;
        if (sale != null)
        {
            sale.Customer = new CustomerInfo
            {
                Name = CustomerName,
                PhoneNumber = PhoneNumber
            };
        }

        await Shell.Current.GoToAsync("../SalePreview");
    }
}

public partial class SalePreviewViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private SaleModel? sale;
    
    [ObservableProperty]
    private bool isBusy;
    
    [ObservableProperty]
    private string stationName = string.Empty;

    public SalePreviewViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public void OnNavigatedTo()
    {
        LoadSale();
    }

    private void LoadSale()
    {
        try
        {
            Sale = AppState.Instance.CurrentSale;
            StationName = AppState.Instance.SelectedStation?.Name ?? "Unknown Station";
            
            System.Diagnostics.Debug.WriteLine($"Sale Preview - Fuel: {Sale?.FuelType}, Liters: {Sale?.Liters}, Amount: {Sale?.AmountRWF}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading sale: {ex}");
        }
    }

    [RelayCommand]
    private async Task Edit()
    {
        try
        {
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Edit error: {ex}");
        }
    }

    [RelayCommand]
    private async Task ConfirmSale()
    {
        if (Sale == null) return;

        IsBusy = true;

        try
        {
            // Simulate API delay
            await Task.Delay(1000);
            
            // Generate receipt details
            Sale.ReceiptNumber = $"RCP{DateTime.Now:yyyyMMddHHmmss}";
            Sale.EBMCode = $"EBM{new Random().Next(100000, 999999)}";

            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("SaleComplete");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Confirm sale error: {ex}");
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to submit sale: {ex.Message}", "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public partial class SaleCompleteViewModel : ObservableObject
{
    [ObservableProperty]
    private SaleModel? sale;

    public SaleCompleteViewModel()
    {
        LoadSale();
    }

    private void LoadSale()
    {
        Sale = AppState.Instance.CurrentSale;
    }

    [RelayCommand]
    private async Task PrintReceipt()
    {
        await Shell.Current.DisplayAlert("Print", "Printing receipt...", "OK");
    }

    [RelayCommand]
    private async Task NewSale()
    {
        AppState.Instance.ClearCurrentSale();
        await Shell.Current.GoToAsync("///NewSale");
    }

    [RelayCommand]
    private async Task GoToDashboard()
    {
        AppState.Instance.ClearCurrentSale();
        await Shell.Current.GoToAsync("///Dashboard");
    }
}
