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
    private DateTime selectedDate = DateTime.Today;

    public TransactionsViewModel(ApiService apiService)
    {
        _apiService = apiService;
        LoadTransactions();
    }

    private async void LoadTransactions()
    {
        await Refresh();
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        LoadTransactions();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        IsRefreshing = true;
        try
        {
            var stationId = AppState.Instance.SelectedStation?.Id ?? Guid.Empty;
            var items = await _apiService.GetTransactionsAsync(stationId, SelectedDate);

            Transactions.Clear();
            foreach (var item in items.OrderByDescending(t => t.TransactionDate))
            {
                Transactions.Add(item);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Refresh transactions error: {ex}");
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load transactions: {ex.Message}", "OK");
            }
        }
        finally
        {
            IsRefreshing = false;
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

    [RelayCommand]
    private async Task LoadMore()
    {
        try
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Info", "Load more functionality will be implemented", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Load more error: {ex}");
        }
    }
}
