using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escale.mobile.Models;
using Escale.mobile.Services;
using System.Collections.ObjectModel;

namespace Escale.mobile.ViewModels;

public partial class NewSaleViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private bool _isUpdating;
    private bool _fuelTypesLoaded;

    [ObservableProperty]
    private ObservableCollection<FuelTypeOption> fuelTypes = new();

    [ObservableProperty]
    private FuelTypeOption? selectedFuelType;

    [ObservableProperty]
    private string amountRWFText = string.Empty;

    [ObservableProperty]
    private string litersText = string.Empty;

    [ObservableProperty]
    private string selectedPaymentMethod = "Cash";

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string calculationDisplay = string.Empty;

    public ObservableCollection<string> PaymentMethods { get; } = new()
    {
        "Cash", "MobileMoney", "Card", "Credit"
    };

    public NewSaleViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    /// <summary>
    /// Called from OnAppearing. Ensures a fresh sale state and loads fuel types.
    /// </summary>
    public async Task InitializeAsync()
    {
        // If no current sale exists, start a fresh one and reset the form
        if (AppState.Instance.CurrentSale == null)
        {
            AppState.Instance.StartNewSale();
            ResetForm();
        }

        // Load fuel types if not yet loaded
        if (!_fuelTypesLoaded)
        {
            await LoadFuelTypesAsync();
        }
    }

    private void ResetForm()
    {
        _isUpdating = true;
        SelectedFuelType = null;
        AmountRWFText = string.Empty;
        LitersText = string.Empty;
        SelectedPaymentMethod = "Cash";
        CalculationDisplay = string.Empty;
        _isUpdating = false;
    }

    partial void OnSelectedFuelTypeChanged(FuelTypeOption? value)
    {
        if (_isUpdating) return;
        // Clear amounts when fuel type changes so user enters fresh values
        _isUpdating = true;
        AmountRWFText = string.Empty;
        LitersText = string.Empty;
        CalculationDisplay = string.Empty;
        _isUpdating = false;
    }

    partial void OnAmountRWFTextChanged(string value)
    {
        if (_isUpdating) return;
        CalculateFromAmount();
    }

    partial void OnLitersTextChanged(string value)
    {
        if (_isUpdating) return;
        CalculateFromLiters();
    }

    private void CalculateFromAmount()
    {
        if (SelectedFuelType == null) return;

        try
        {
            _isUpdating = true;
            if (!string.IsNullOrWhiteSpace(AmountRWFText) && decimal.TryParse(AmountRWFText, out var amount) && amount > 0)
            {
                var liters = amount / SelectedFuelType.PricePerLiter;
                LitersText = liters.ToString("F2");
                var total = amount;
                var vat = Math.Round(total * 0.18m, 0);
                var subtotal = total - vat;
                CalculationDisplay = $"{liters:F2} L\nSubtotal: RWF {subtotal:N0}\nVAT (18%): RWF {vat:N0}\nTotal: RWF {total:N0}";
            }
            else
            {
                LitersText = string.Empty;
                CalculationDisplay = string.Empty;
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void CalculateFromLiters()
    {
        if (SelectedFuelType == null) return;

        try
        {
            _isUpdating = true;
            if (!string.IsNullOrWhiteSpace(LitersText) && decimal.TryParse(LitersText, out var liters) && liters > 0)
            {
                var amount = liters * SelectedFuelType.PricePerLiter;
                AmountRWFText = amount.ToString("F0");
                var total = amount;
                var vat = Math.Round(total * 0.18m, 0);
                var subtotal = total - vat;
                CalculationDisplay = $"{liters:F2} L\nSubtotal: RWF {subtotal:N0}\nVAT (18%): RWF {vat:N0}\nTotal: RWF {total:N0}";
            }
            else
            {
                AmountRWFText = string.Empty;
                CalculationDisplay = string.Empty;
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private async Task LoadFuelTypesAsync()
    {
        try
        {
            IsBusy = true;
            var types = await _apiService.GetFuelTypesAsync();

            FuelTypes.Clear();
            foreach (var type in types)
            {
                FuelTypes.Add(type);
            }

            _fuelTypesLoaded = types.Count > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading fuel types: {ex}");
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load fuel types: {ex.Message}", "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Continue()
    {
        try
        {
            // Validate fuel type
            if (SelectedFuelType == null)
            {
                await Shell.Current!.DisplayAlert("Required", "Please select a fuel type.", "OK");
                return;
            }

            // Validate amount/liters
            if (string.IsNullOrWhiteSpace(LitersText) || !decimal.TryParse(LitersText, out var liters) || liters <= 0)
            {
                await Shell.Current!.DisplayAlert("Required", "Please enter a valid amount or liters.", "OK");
                return;
            }

            // Set sale data
            var sale = AppState.Instance.CurrentSale;
            if (sale == null)
            {
                AppState.Instance.StartNewSale();
                sale = AppState.Instance.CurrentSale!;
            }

            sale.FuelType = SelectedFuelType.Name;
            sale.FuelTypeId = SelectedFuelType.Id;
            sale.PricePerLiter = SelectedFuelType.PricePerLiter;
            sale.Liters = liters;
            sale.AmountRWF = decimal.TryParse(AmountRWFText, out var amt) ? amt : liters * SelectedFuelType.PricePerLiter;
            sale.PaymentMethod = SelectedPaymentMethod;

            // Navigate based on payment method
            if (SelectedPaymentMethod == "Credit")
            {
                await Shell.Current!.GoToAsync("SubscriptionCustomerSearch");
            }
            else
            {
                await Shell.Current!.GoToAsync("CustomerInfo");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Continue error: {ex}");
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }

    [RelayCommand]
    private async Task Cancel()
    {
        try
        {
            AppState.Instance.ClearCurrentSale();
            ResetForm();
            await Shell.Current!.GoToAsync("///Dashboard");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cancel error: {ex}");
        }
    }
}
