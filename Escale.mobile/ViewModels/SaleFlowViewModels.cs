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

    /// <summary>
    /// Reset form when navigated to, so previous customer data doesn't linger.
    /// </summary>
    public void OnNavigatedTo()
    {
        var sale = AppState.Instance.CurrentSale;
        if (sale?.Customer != null)
        {
            CustomerName = sale.Customer.Name;
            PhoneNumber = sale.Customer.PhoneNumber ?? string.Empty;
        }
        else
        {
            CustomerName = string.Empty;
            PhoneNumber = string.Empty;
        }
    }

    [RelayCommand]
    private async Task Skip()
    {
        // Clear any customer data — this is a walk-in sale
        var sale = AppState.Instance.CurrentSale;
        if (sale != null)
        {
            sale.Customer = null;
        }
        await Shell.Current.GoToAsync("SalePreview");
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
            // Show confirmation dialog before submitting
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

            if (success && completedSale != null)
            {
                Sale.Id = completedSale.Id;
                Sale.ReceiptNumber = completedSale.ReceiptNumber;
                Sale.EBMCode = completedSale.EBMCode;
                Sale.TransactionDate = completedSale.TransactionDate;
                Sale.SubscriptionDeduction = completedSale.SubscriptionDeduction;
                Sale.SubscriptionRemainingBalance = completedSale.SubscriptionRemainingBalance;

                await Shell.Current!.GoToAsync("SaleComplete");
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
}

public partial class SaleCompleteViewModel : ObservableObject
{
    [ObservableProperty]
    private SaleModel? sale;

    [ObservableProperty]
    private string stationName = string.Empty;

    public SaleCompleteViewModel()
    {
    }

    /// <summary>
    /// Called from code-behind OnNavigatedTo to load fresh sale data.
    /// </summary>
    public void OnNavigatedTo()
    {
        Sale = AppState.Instance.CurrentSale;
        StationName = AppState.Instance.SelectedStation?.Name ?? "Unknown Station";
    }

    [RelayCommand]
    private async Task PrintReceipt()
    {
        await Shell.Current!.DisplayAlert("Print", "Printing receipt...", "OK");
    }

    [RelayCommand]
    private async Task NewSale()
    {
        try
        {
            // Clear completed sale and start fresh
            AppState.Instance.ClearCurrentSale();

            // Navigate to NewSale tab (absolute route resets the stack)
            await Shell.Current!.GoToAsync("///NewSale");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"NewSale navigation error: {ex}");
        }
    }

    [RelayCommand]
    private async Task GoToDashboard()
    {
        try
        {
            // Clear completed sale
            AppState.Instance.ClearCurrentSale();

            // Navigate to Dashboard tab (absolute route resets the stack)
            await Shell.Current!.GoToAsync("///Dashboard");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dashboard navigation error: {ex}");
        }
    }
}
