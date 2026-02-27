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
        if (sale != null && !string.IsNullOrWhiteSpace(CustomerName))
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
            var stationId = AppState.Instance.SelectedStation?.Id ?? Guid.Empty;
            var (success, message, completedSale) = await _apiService.SubmitSaleAsync(Sale, stationId);

            if (success && completedSale != null)
            {
                Sale.Id = completedSale.Id;
                Sale.ReceiptNumber = completedSale.ReceiptNumber;
                Sale.EBMCode = completedSale.EBMCode;
                Sale.TransactionDate = completedSale.TransactionDate;

                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("SaleComplete");
                }
            }
            else
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Error", message, "OK");
                }
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

    [ObservableProperty]
    private string stationName = string.Empty;

    public SaleCompleteViewModel()
    {
        LoadSale();
    }

    private void LoadSale()
    {
        Sale = AppState.Instance.CurrentSale;
        StationName = AppState.Instance.SelectedStation?.Name ?? "Unknown Station";
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
