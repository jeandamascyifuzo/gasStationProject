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

    [ObservableProperty]
    private string tin = string.Empty;

    public CustomerInfoViewModel()
    {
    }

    public void OnNavigatedTo()
    {
        var sale = AppState.Instance.CurrentSale;
        if (sale?.Customer != null)
        {
            CustomerName = sale.Customer.Name;
            PhoneNumber = sale.Customer.PhoneNumber ?? string.Empty;
            Tin = sale.Customer.TIN ?? string.Empty;
        }
        else
        {
            CustomerName = string.Empty;
            PhoneNumber = string.Empty;
            Tin = string.Empty;
        }
    }

    [RelayCommand]
    private async Task Skip()
    {
        var sale = AppState.Instance.CurrentSale;
        if (sale != null)
        {
            sale.Customer = new CustomerInfo
            {
                Name = "Walk-in",
                PhoneNumber = "0781917267"
            };
        }
        await Shell.Current.GoToAsync("SalePreview");
    }

    [RelayCommand]
    private async Task Continue()
    {
        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            await Shell.Current.DisplayAlert("Required", "Please enter customer name.", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(PhoneNumber))
        {
            await Shell.Current.DisplayAlert("Required", "Please enter phone number.", "OK");
            return;
        }

        var sale = AppState.Instance.CurrentSale;
        if (sale != null)
        {
            sale.Customer = new CustomerInfo
            {
                Name = CustomerName.Trim(),
                PhoneNumber = PhoneNumber.Trim(),
                TIN = string.IsNullOrWhiteSpace(Tin) ? null : Tin.Trim()
            };
        }

        await Shell.Current.GoToAsync("SalePreview");
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

    [ObservableProperty]
    private decimal balanceAfterSale;

    // Success popup state
    [ObservableProperty]
    private bool showSuccessPopup;

    [ObservableProperty]
    private string completedReceiptNumber = string.Empty;

    [ObservableProperty]
    private string completedFuelType = string.Empty;

    [ObservableProperty]
    private string completedLiters = string.Empty;

    [ObservableProperty]
    private string completedTotal = string.Empty;

    [ObservableProperty]
    private string completedPayment = string.Empty;

    [ObservableProperty]
    private bool hasEBMReceipt;

    private string? _ebmReceiptUrl;

    public SalePreviewViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public void OnNavigatedTo()
    {
        ShowSuccessPopup = false;
        LoadSale();
    }

    private void LoadSale()
    {
        try
        {
            Sale = AppState.Instance.CurrentSale;
            StationName = AppState.Instance.SelectedStation?.Name ?? "Unknown Station";

            if (Sale?.SubscriptionId != null && Sale.Customer?.SubscriptionBalance != null)
            {
                BalanceAfterSale = (Sale.Customer.SubscriptionBalance ?? 0) - Sale.Total;
            }
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

        try
        {
            var confirm = await Shell.Current!.DisplayAlert(
                "Confirm Sale",
                $"Fuel: {Sale.FuelType}\n" +
                $"Liters: {Sale.Liters:F2} L\n" +
                $"Total: RWF {Sale.Total:N0}\n" +
                $"Payment: {Sale.PaymentMethod}\n\n" +
                "Do you want to confirm this sale?",
                "Confirm",
                "Cancel");

            if (!confirm) return;

            IsBusy = true;

            var stationId = AppState.Instance.SelectedStation?.Id ?? Guid.Empty;
            var (success, message, completedSale) = await _apiService.SubmitSaleAsync(Sale, stationId);

            IsBusy = false;

            if (success && completedSale != null)
            {
                CompletedReceiptNumber = completedSale.ReceiptNumber;
                CompletedFuelType = completedSale.FuelType;
                CompletedLiters = $"{completedSale.Liters:F2} L";
                CompletedTotal = $"RWF {completedSale.Total:N0}";
                CompletedPayment = completedSale.PaymentMethod;
                HasEBMReceipt = !string.IsNullOrEmpty(completedSale.EBMReceiptUrl);
                _ebmReceiptUrl = completedSale.EBMReceiptUrl;
                ShowSuccessPopup = true;
            }
            else
            {
                await Shell.Current!.DisplayAlert("Error", message, "OK");
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

    [RelayCommand]
    private async Task PrintReceipt()
    {
        if (!string.IsNullOrEmpty(_ebmReceiptUrl))
        {
            try
            {
                await Browser.Default.OpenAsync(
                    new Uri(_ebmReceiptUrl),
                    BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Open receipt error: {ex}");
                if (Shell.Current != null)
                    await Shell.Current.DisplayAlert("Error", "Could not open receipt", "OK");
            }
        }
    }

    [RelayCommand]
    private async Task NewSale()
    {
        try
        {
            ShowSuccessPopup = false;
            AppState.Instance.ClearCurrentSale();
            await Shell.Current!.GoToAsync("///NewSale");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"NewSale navigation error: {ex}");
        }
    }
}

