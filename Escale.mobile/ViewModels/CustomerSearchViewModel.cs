using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escale.mobile.Models;
using Escale.mobile.Services;
using System.Collections.ObjectModel;

namespace Escale.mobile.ViewModels;

public partial class CustomerSearchViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    
    [ObservableProperty]
    private string searchTerm = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<CustomerInfo> searchResults = new();
    
    [ObservableProperty]
    private bool isSearching;
    
    [ObservableProperty]
    private bool hasSearched;

    public CustomerSearchViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task Search()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            await Shell.Current.DisplayAlert("Validation", "Please enter plate number or car PIN", "OK");
            return;
        }

        IsSearching = true;
        HasSearched = true;

        try
        {
            var results = await _apiService.SearchCustomersAsync(SearchTerm);
            
            SearchResults.Clear();
            foreach (var customer in results)
            {
                SearchResults.Add(customer);
            }

            if (SearchResults.Count == 0)
            {
                await Shell.Current.DisplayAlert("No Results", "No customer found with that search term", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Search failed: {ex.Message}", "OK");
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private async Task SelectCustomer(CustomerInfo customer)
    {
        var sale = AppState.Instance.CurrentSale;
        if (sale != null)
        {
            sale.Customer = customer;
        }

        await Shell.Current.GoToAsync("../SalePreview");
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Shell.Current.GoToAsync("..");
    }
}
