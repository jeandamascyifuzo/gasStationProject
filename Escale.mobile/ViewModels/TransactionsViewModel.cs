using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escale.mobile.Models;
using Escale.mobile.Services;
using System.Collections.ObjectModel;

namespace Escale.mobile.ViewModels;

public partial class TransactionsViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<Transaction> transactions = new();

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool hasData;

    [ObservableProperty]
    private decimal totalSales;

    [ObservableProperty]
    private DateTime selectedDate = DateTime.Today;

    public TransactionsViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        _ = Refresh();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        if (!IsRefreshing)
            IsLoading = true;

        try
        {
            var stationId = AppState.Instance.SelectedStation?.Id ?? Guid.Empty;
            var items = await _apiService.GetTransactionsAsync(stationId, SelectedDate);

            Transactions.Clear();
            decimal sum = 0;
            foreach (var item in items.OrderByDescending(t => t.TransactionDate))
            {
                Transactions.Add(item);
                sum += item.Total;
            }
            TotalSales = sum;
            HasData = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Refresh transactions error: {ex}");
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load transactions: {ex.Message}", "OK");
            }
            HasData = false;
        }
        finally
        {
            IsRefreshing = false;
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ViewDetails(Transaction? transaction)
    {
        try
        {
            if (transaction == null || Shell.Current == null) return;

            await Shell.Current.DisplayAlert(
                "Transaction Details",
                $"Receipt: {transaction.ReceiptNumber}\n" +
                $"Fuel: {transaction.FuelType} ({transaction.Liters:F2} L)\n" +
                $"Total: RWF {transaction.Total:N0}\n" +
                $"Payment: {transaction.PaymentMethod}\n" +
                $"Customer: {transaction.CustomerName ?? "Walk-in"}\n" +
                $"EBM: {(transaction.EBMSent ? transaction.EBMCode : "Pending")}",
                "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"View details error: {ex}");
        }
    }
}
