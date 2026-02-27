using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escale.mobile.Models;
using Escale.mobile.Services;
using Escale.mobile.Views;

namespace Escale.mobile.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    private string userRole = string.Empty;

    [ObservableProperty]
    private string stationName = string.Empty;

    [ObservableProperty]
    private bool isClockedIn;

    [ObservableProperty]
    private DateTime? shiftStartTime;

    [ObservableProperty]
    private int todaysTransactions;

    [ObservableProperty]
    private decimal todaysAmount;

    [ObservableProperty]
    private bool canSwitchStation;

    public ProfileViewModel(ApiService apiService)
    {
        _apiService = apiService;
        LoadProfileData();
    }

    private async void LoadProfileData()
    {
        try
        {
            UserName = AppState.Instance.CurrentUser?.FullName ?? "User";
            UserRole = AppState.Instance.CurrentUser?.Role ?? "Cashier";
            StationName = AppState.Instance.SelectedStation?.Name ?? "Station";
            IsClockedIn = AppState.Instance.IsClockedIn;
            ShiftStartTime = AppState.Instance.ShiftStartTime;

            // Admin and Manager can switch stations
            var role = AppState.Instance.CurrentUser?.Role;
            CanSwitchStation = role == "Admin" || role == "Manager";

            await LoadShiftSummary();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading profile data: {ex}");
        }
    }

    private async Task LoadShiftSummary()
    {
        try
        {
            var userId = AppState.Instance.CurrentUser?.Id ?? Guid.Empty;
            var stationId = AppState.Instance.SelectedStation?.Id ?? Guid.Empty;

            if (userId == Guid.Empty || stationId == Guid.Empty) return;

            var summary = await _apiService.GetShiftSummaryAsync(userId, stationId);

            TodaysTransactions = summary.TransactionCount;
            TodaysAmount = summary.TotalSales;

            if (summary.ShiftStart != default)
            {
                ShiftStartTime = summary.ShiftStart;
                IsClockedIn = summary.ShiftEnd == null;
                if (IsClockedIn && !AppState.Instance.IsClockedIn)
                {
                    AppState.Instance.ClockIn();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading shift summary: {ex}");
            TodaysTransactions = 0;
            TodaysAmount = 0;
        }
    }

    [RelayCommand]
    private async Task SwitchStation()
    {
        try
        {
            if (Shell.Current == null) return;

            var stations = AppState.Instance.CurrentUser?.AssignedStations;
            if (stations == null || stations.Count == 0)
            {
                await Shell.Current.DisplayAlert("No Stations", "No stations assigned to your account.", "OK");
                return;
            }

            var stationNames = stations.Select(s => s.Name).ToArray();
            var selected = await Shell.Current.DisplayActionSheet(
                "Select Station", "Cancel", null, stationNames);

            if (selected == null || selected == "Cancel") return;

            var station = stations.FirstOrDefault(s => s.Name == selected);
            if (station != null)
            {
                AppState.Instance.SetStation(station);
                StationName = station.Name;

                // Reload shift data for the new station
                await LoadShiftSummary();

                await Shell.Current.DisplayAlert("Station Changed",
                    $"Now operating at: {station.Name}", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Switch station error: {ex}");
        }
    }

    [RelayCommand]
    private async Task ClockInOut()
    {
        var isClockingIn = !IsClockedIn;
        var action = isClockingIn ? "clock in" : "clock out";

        try
        {
            if (Shell.Current == null) return;

            var confirm = await Shell.Current.DisplayAlert(
                "Confirm",
                $"Are you sure you want to {action}?",
                "Yes",
                "No");

            if (!confirm) return;

            var userId = AppState.Instance.CurrentUser?.Id ?? Guid.Empty;
            var stationId = AppState.Instance.SelectedStation?.Id ?? Guid.Empty;

            var (success, message) = await _apiService.ClockInOutAsync(userId, stationId, isClockingIn);

            if (success)
            {
                if (isClockingIn)
                {
                    AppState.Instance.ClockIn();
                    ShiftStartTime = DateTime.Now;
                }
                else
                {
                    AppState.Instance.ClockOut();
                    ShiftStartTime = null;
                }

                IsClockedIn = isClockingIn;
                await Shell.Current.DisplayAlert("Success", $"Successfully {action}ed", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", message, "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Clock in/out error: {ex}");
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }

    [RelayCommand]
    private async Task ShiftSummary()
    {
        try
        {
            await LoadShiftSummary();
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert(
                    "Shift Summary",
                    $"Transactions: {TodaysTransactions}\n" +
                    $"Total Sales: RWF {TodaysAmount:N0}\n" +
                    $"Shift Start: {ShiftStartTime?.ToString("HH:mm") ?? "N/A"}",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Shift summary error: {ex}");
        }
    }

    [RelayCommand]
    private async Task ChangePassword()
    {
        try
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert("Coming Soon", "Change password feature will be available soon", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Change password error: {ex}");
        }
    }

    [RelayCommand]
    private async Task Logout()
    {
        try
        {
            if (Shell.Current == null) return;

            var confirm = await Shell.Current.DisplayAlert(
                "Logout",
                "Are you sure you want to logout?",
                "Yes",
                "No");

            if (confirm)
            {
                AppState.Instance.Logout();

                Application.Current!.MainPage = new NavigationPage(
                    MauiProgram.Services.GetRequiredService<LoginPage>());
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Logout error: {ex}");
        }
    }
}
