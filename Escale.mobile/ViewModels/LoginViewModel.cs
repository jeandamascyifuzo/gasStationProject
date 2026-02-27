using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escale.mobile.Models;
using Escale.mobile.Services;

namespace Escale.mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool rememberMe;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public LoginViewModel(ApiService apiService)
    {
        _apiService = apiService;
        LoadSavedCredentials();
    }

    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter username and password";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var loginModel = new LoginModel
            {
                Username = Username,
                Password = Password,
                RememberMe = RememberMe
            };

            var response = await _apiService.LoginAsync(loginModel);

            if (response?.Success == true && response.User != null)
            {
                AppState.Instance.SetUser(response.User, response.Token ?? string.Empty);

                if (RememberMe)
                {
                    await SaveCredentials();
                }

                if (response.User.AssignedStations == null || response.User.AssignedStations.Count == 0)
                {
                    ErrorMessage = "No stations assigned to this user. Contact your administrator.";
                    return;
                }

                var role = response.User.Role;

                if (role == "Cashier")
                {
                    // Cashier: auto-assign to their station, go straight to dashboard
                    AppState.Instance.SetStation(response.User.AssignedStations[0]);
                    Application.Current!.MainPage = new AppShell();
                }
                else
                {
                    // Admin/Manager: let them choose which station to work at
                    Application.Current!.MainPage = new AppShell();
                    await Shell.Current.GoToAsync("///StationSelection");
                }
            }
            else
            {
                ErrorMessage = response?.Message ?? "Login failed. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Login Error: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LoadSavedCredentials()
    {
        if (Preferences.ContainsKey("RememberMe"))
        {
            RememberMe = Preferences.Get("RememberMe", false);
            if (RememberMe)
            {
                Username = Preferences.Get("SavedUsername", string.Empty);
            }
        }
    }

    private async Task SaveCredentials()
    {
        Preferences.Set("RememberMe", RememberMe);
        Preferences.Set("SavedUsername", Username);
        await SecureStorage.SetAsync("SavedPassword", Password);
    }
}
