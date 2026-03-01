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
    private DateTime startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime endDate = DateTime.Today;

    public TransactionsViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    partial void OnStartDateChanged(DateTime value)
    {
        if (value > EndDate)
            EndDate = value;
        _ = Refresh();
    }

    partial void OnEndDateChanged(DateTime value)
    {
        if (value < StartDate)
            StartDate = value;
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
            var items = await _apiService.GetTransactionsAsync(stationId, StartDate, EndDate);

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

            var ebmStatus = transaction.EBMSent ? "Sent" : "Pending";

            // If EBM receipt URL is available, offer to view it
            if (transaction.EBMSent && !string.IsNullOrEmpty(transaction.EBMReceiptUrl))
            {
                var action = await Shell.Current.DisplayActionSheet(
                    $"Receipt: {transaction.ReceiptNumber}\n" +
                    $"Fuel: {transaction.FuelType} ({transaction.Liters:F2} L)\n" +
                    $"Total: RWF {transaction.Total:N0}\n" +
                    $"Payment: {transaction.PaymentMethod}\n" +
                    $"Customer: {transaction.CustomerName ?? "Walk-in"}\n" +
                    $"EBM: {ebmStatus}",
                    "Close",
                    null,
                    "View EBM Receipt");

                if (action == "View EBM Receipt")
                {
                    await Browser.Default.OpenAsync(
                        new Uri(transaction.EBMReceiptUrl),
                        BrowserLaunchMode.SystemPreferred);
                }
            }
            else
            {
                await Shell.Current.DisplayAlert(
                    "Transaction Details",
                    $"Receipt: {transaction.ReceiptNumber}\n" +
                    $"Fuel: {transaction.FuelType} ({transaction.Liters:F2} L)\n" +
                    $"Total: RWF {transaction.Total:N0}\n" +
                    $"Payment: {transaction.PaymentMethod}\n" +
                    $"Customer: {transaction.CustomerName ?? "Walk-in"}\n" +
                    $"EBM: {ebmStatus}",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"View details error: {ex}");
        }
    }
}
