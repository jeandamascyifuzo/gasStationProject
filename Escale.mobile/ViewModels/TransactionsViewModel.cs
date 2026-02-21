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

    [RelayCommand]
    private async Task Refresh()
    {
        IsRefreshing = true;
        try
        {
            // Simulate API delay
            await Task.Delay(800);
            
            // Load dummy transaction data
            var items = GetDummyTransactions();
            
            Transactions.Clear();
            foreach (var item in items)
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

    private List<Transaction> GetDummyTransactions()
    {
        try
        {
            var random = new Random();
            var transactions = new List<Transaction>();
            var fuelTypes = new[] { "Petrol 95", "Petrol 98", "Diesel", "Kerosene" };
            var paymentMethods = new[] { "Cash", "MoMo", "Card", "Credit" };
            var customers = new[] { null, "John Doe", "Jane Smith", "ABC Company", null, null, "Private Customer" };

            // Generate 15-25 dummy transactions for today
            int count = random.Next(15, 26);
            var baseTime = DateTime.Today.AddHours(7); // Start at 7 AM

            for (int i = 0; i < count; i++)
            {
                var fuelType = fuelTypes[random.Next(fuelTypes.Length)];
                var pricePerLiter = fuelType switch
                {
                    "Petrol 95" => 1450,
                    "Petrol 98" => 1550,
                    "Diesel" => 1380,
                    "Kerosene" => 1200,
                    _ => 1400
                };

                var liters = random.Next(5, 60);
                var total = liters * pricePerLiter;
                
                transactions.Add(new Transaction
                {
                    Id = i + 1,
                    ReceiptNumber = $"RCP{DateTime.Now:yyyyMMdd}{(1000 + i):D4}",
                    TransactionDate = baseTime.AddMinutes(random.Next(0, 720)), // Random time during day
                    FuelType = fuelType,
                    Liters = liters,
                    Total = total,
                    PaymentMethod = paymentMethods[random.Next(paymentMethods.Length)],
                    CustomerName = customers[random.Next(customers.Length)],
                    EBMSent = random.Next(100) > 10, // 90% sent
                    EBMCode = random.Next(100) > 10 ? $"EBM{random.Next(100000, 999999)}" : null
                });
            }

            return transactions.OrderByDescending(t => t.TransactionDate).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error generating dummy transactions: {ex}");
            return new List<Transaction>();
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
