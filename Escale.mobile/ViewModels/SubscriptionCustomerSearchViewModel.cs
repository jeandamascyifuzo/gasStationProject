using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escale.mobile.Models;
using Escale.mobile.Services;

namespace Escale.mobile.ViewModels;

public partial class SubscriptionCustomerSearchViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string plateNumber = string.Empty;

    [ObservableProperty]
    private string carPIN = string.Empty;

    [ObservableProperty]
    private bool isSearching;

    [ObservableProperty]
    private bool hasResult;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string? customerName;

    [ObservableProperty]
    private string? customerPhone;

    [ObservableProperty]
    private string? resultPlateNumber;

    [ObservableProperty]
    private string? vehicleDescription;

    [ObservableProperty]
    private decimal subscriptionBalance;

    [ObservableProperty]
    private string? subscriptionExpiry;

    [ObservableProperty]
    private bool hasSufficientBalance;

    private SubscriptionCustomerLookupResponse? _lookupResult;

    public SubscriptionCustomerSearchViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public void OnNavigatedTo()
    {
        PlateNumber = string.Empty;
        CarPIN = string.Empty;
        HasResult = false;
        ErrorMessage = null;
        _lookupResult = null;
    }

    [RelayCommand]
    private async Task Search()
    {
        if (string.IsNullOrWhiteSpace(PlateNumber))
        {
            ErrorMessage = "Please enter a plate number.";
            return;
        }

        if (string.IsNullOrWhiteSpace(CarPIN) || CarPIN.Length != 4)
        {
            ErrorMessage = "Please enter a valid 4-digit PIN.";
            return;
        }

        try
        {
            IsSearching = true;
            ErrorMessage = null;
            HasResult = false;

            var saleAmount = AppState.Instance.CurrentSale?.Total;
            var (success, error, data) = await _apiService.LookupSubscriptionCarAsync(PlateNumber.Trim(), CarPIN, saleAmount);

            if (success && data != null)
            {
                _lookupResult = data;
                CustomerName = data.CustomerName;
                CustomerPhone = data.PhoneNumber;
                ResultPlateNumber = data.PlateNumber;
                VehicleDescription = $"{data.CarMake} {data.CarModel}".Trim();
                SubscriptionBalance = data.RemainingBalance ?? 0;
                SubscriptionExpiry = data.SubscriptionExpiryDate?.ToString("MMM dd, yyyy");
                HasSufficientBalance = data.HasSufficientBalance;
                HasResult = true;
            }
            else
            {
                ErrorMessage = error ?? "Customer not found.";
                HasResult = false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private async Task SelectCustomer()
    {
        if (_lookupResult == null) return;

        var sale = AppState.Instance.CurrentSale;
        if (sale == null) return;

        if (!HasSufficientBalance)
        {
            await Shell.Current!.DisplayAlert("Insufficient Balance",
                $"Subscription balance (RWF {SubscriptionBalance:N0}) is less than sale total (RWF {sale.Total:N0}).",
                "OK");
            return;
        }

        // Set customer and subscription info on the sale
        sale.Customer = new CustomerInfo
        {
            Id = _lookupResult.CustomerId,
            Name = _lookupResult.CustomerName,
            PhoneNumber = _lookupResult.PhoneNumber ?? string.Empty,
            PlateNumber = _lookupResult.PlateNumber,
            CarId = _lookupResult.CarId,
            VehicleModel = $"{_lookupResult.CarMake} {_lookupResult.CarModel}".Trim(),
            ActiveSubscriptionId = _lookupResult.ActiveSubscriptionId,
            SubscriptionBalance = _lookupResult.RemainingBalance,
            SubscriptionExpiryDate = _lookupResult.SubscriptionExpiryDate
        };
        sale.SubscriptionId = _lookupResult.ActiveSubscriptionId;

        await Shell.Current!.GoToAsync("SalePreview");
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Shell.Current!.GoToAsync("..");
    }
}
