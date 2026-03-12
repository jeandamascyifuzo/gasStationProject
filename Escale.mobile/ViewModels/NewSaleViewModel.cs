using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escale.mobile.Models;
using Escale.mobile.Services;
using System.Collections.ObjectModel;

namespace Escale.mobile.ViewModels;

public partial class NewSaleViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly SignalRService _signalRService;
    private Action<string>? _dataChangedHandler;
    private bool _isUpdating;
    private bool _fuelTypesLoaded;
    private bool _paymentMethodsLoaded;

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
    private PaymentMethodOption? selectedPaymentMethodOption;

    [ObservableProperty]
    private bool isBusy;

    public ObservableCollection<PaymentMethodOption> PaymentMethods { get; } = new();

    public NewSaleViewModel(ApiService apiService, SignalRService signalRService)
    {
        _apiService = apiService;
        _signalRService = signalRService;
    }

    public void SubscribeToNotifications()
    {
        UnsubscribeFromNotifications();
        _dataChangedHandler = changeType =>
        {
            if (changeType == NotificationConstants.FuelTypesChanged)
            {
                _fuelTypesLoaded = false;
                _apiService.InvalidateFuelTypesCache();
                MainThread.BeginInvokeOnMainThread(async () => await InitializeAsync());
            }
        };
        _signalRService.OnDataChanged += _dataChangedHandler;
    }

    public void UnsubscribeFromNotifications()
    {
        if (_dataChangedHandler != null)
        {
            _signalRService.OnDataChanged -= _dataChangedHandler;
            _dataChangedHandler = null;
        }
    }

    /// <summary>
    /// Called from OnAppearing. Ensures a fresh sale state and loads fuel types.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Always start a fresh sale when the page appears (e.g. tab clicked)
        AppState.Instance.ClearCurrentSale();
        AppState.Instance.StartNewSale();
        ResetForm();

        // Load payment methods and fuel types if not yet loaded
        var tasks = new List<Task>();
        if (!_paymentMethodsLoaded)
            tasks.Add(LoadPaymentMethodsAsync());
        if (!_fuelTypesLoaded)
            tasks.Add(LoadFuelTypesAsync());
        if (tasks.Count > 0)
            await Task.WhenAll(tasks);
    }

    private void ResetForm()
    {
        _isUpdating = true;
        SelectedFuelType = null;
        AmountRWFText = string.Empty;
        LitersText = string.Empty;
        SelectedPaymentMethodOption = PaymentMethods.FirstOrDefault(p => p.Name == "Cash")
            ?? PaymentMethods.FirstOrDefault();
        _isUpdating = false;
    }

    partial void OnSelectedPaymentMethodOptionChanged(PaymentMethodOption? value)
    {
        SelectedPaymentMethod = value?.Name ?? string.Empty;
    }

    partial void OnSelectedFuelTypeChanged(FuelTypeOption? value)
    {
        if (_isUpdating) return;
        // Clear amounts when fuel type changes so user enters fresh values
        _isUpdating = true;
        AmountRWFText = string.Empty;
        LitersText = string.Empty;
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
            }
            else
            {
                LitersText = string.Empty;
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
            }
            else
            {
                AmountRWFText = string.Empty;
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private async Task LoadPaymentMethodsAsync()
    {
        try
        {
            var methods = await _apiService.GetEnabledPaymentMethodsAsync();
            PaymentMethods.Clear();
            foreach (var m in methods)
                PaymentMethods.Add(m);

            _paymentMethodsLoaded = PaymentMethods.Count > 0;

            // Select Cash by default, or the first available method
            SelectedPaymentMethodOption = PaymentMethods.FirstOrDefault(p => p.Name == "Cash")
                ?? PaymentMethods.FirstOrDefault();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading payment methods: {ex}");
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
