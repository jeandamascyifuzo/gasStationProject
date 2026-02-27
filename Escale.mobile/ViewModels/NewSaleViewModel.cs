using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escale.mobile.Models;
using Escale.mobile.Services;
using System.Collections.ObjectModel;

namespace Escale.mobile.ViewModels;

public partial class NewSaleViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private bool _isUpdating = false;

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
        LoadFuelTypes();
    }

    partial void OnSelectedFuelTypeChanged(FuelTypeOption? value)
    {
        UpdateCalculation();
    }

    partial void OnAmountRWFTextChanged(string value)
    {
        if (_isUpdating) return;
        UpdateCalculation();
    }

    partial void OnLitersTextChanged(string value)
    {
        if (_isUpdating) return;
        UpdateCalculation();
    }

    private void UpdateCalculation()
    {
        if (SelectedFuelType == null)
        {
            CalculationDisplay = string.Empty;
            return;
        }

        try
        {
            _isUpdating = true;

            if (!string.IsNullOrWhiteSpace(AmountRWFText) && decimal.TryParse(AmountRWFText, out var amount))
            {
                var calculatedLiters = amount / SelectedFuelType.PricePerLiter;
                LitersText = calculatedLiters.ToString("F2");
                CalculationDisplay = $"{amount:N0} RWF = {calculatedLiters:F2} Liters";
            }
            else if (!string.IsNullOrWhiteSpace(LitersText) && decimal.TryParse(LitersText, out var litersValue))
            {
                var calculatedAmount = litersValue * SelectedFuelType.PricePerLiter;
                AmountRWFText = calculatedAmount.ToString("F0");
                CalculationDisplay = $"{litersValue:F2} Liters = {calculatedAmount:N0} RWF";
            }
            else
            {
                CalculationDisplay = string.Empty;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Calculation error: {ex}");
            CalculationDisplay = string.Empty;
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private async void LoadFuelTypes()
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
            if (SelectedFuelType == null)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Validation", "Please select a fuel type", "OK");
                }
                return;
            }

            if (string.IsNullOrWhiteSpace(AmountRWFText) && string.IsNullOrWhiteSpace(LitersText))
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Validation", "Please enter amount or liters", "OK");
                }
                return;
            }

            decimal? amountValue = null;
            decimal? litersValue = null;

            if (!string.IsNullOrWhiteSpace(AmountRWFText) && decimal.TryParse(AmountRWFText, out var amt))
            {
                amountValue = amt;
            }

            if (!string.IsNullOrWhiteSpace(LitersText) && decimal.TryParse(LitersText, out var ltr))
            {
                litersValue = ltr;
            }

            var sale = AppState.Instance.CurrentSale;
            if (sale != null)
            {
                sale.FuelType = SelectedFuelType.Name;
                sale.FuelTypeId = SelectedFuelType.Id;
                sale.PricePerLiter = SelectedFuelType.PricePerLiter;
                sale.AmountRWF = amountValue;
                sale.Liters = litersValue;
                sale.PaymentMethod = SelectedPaymentMethod;
            }

            if (Shell.Current != null)
            {
                if (SelectedPaymentMethod == "Credit")
                {
                    await Shell.Current.GoToAsync("CustomerSearch");
                }
                else
                {
                    await Shell.Current.GoToAsync("CustomerInfo");
                }
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
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("///Dashboard");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Cancel error: {ex}");
        }
    }
}
