using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escale.mobile.Models;
using Escale.mobile.Services;

namespace Escale.mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly SignalRService _signalRService;

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

    public LoginViewModel(ApiService apiService, SignalRService signalRService)
    {
        _apiService = apiService;
        _signalRService = signalRService;
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
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var loginModel = new LoginModel
            {
                Username = Username,
                Password = Password,
                RememberMe = RememberMe
            };

            var response = await _apiService.LoginAsync(loginModel);
            System.Diagnostics.Debug.WriteLine($"[Login] API call took {sw.ElapsedMilliseconds}ms");

            if (response?.Success == true && response.User != null)
            {
                AppState.Instance.SetUser(response.User, response.Token ?? string.Empty);

                // Fire-and-forget: SignalR, fuel types preload, and credential saving
                var token = response.Token ?? string.Empty;
                if (!string.IsNullOrEmpty(token))
                    _ = _signalRService.ConnectAsync(token);
                _ = _apiService.GetFuelTypesAsync();
                if (RememberMe)
                    _ = SaveCredentials();

                if (response.User.AssignedStations == null || response.User.AssignedStations.Count == 0)
                {
                    ErrorMessage = "No stations assigned to this user. Contact your administrator.";
                    return;
                }

                if (response.User.Role == "Cashier")
                {
                    AppState.Instance.SetStation(response.User.AssignedStations[0]);
                    Application.Current!.Windows[0].Page = App.GetOrCreateShell();
                }
                else
                {
                    Application.Current!.Windows[0].Page = App.GetOrCreateShell();
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
