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
            // Use dummy data for testing (no API required)
            var response = GetDummyLoginResponse();

            if (response?.Success == true && response.User != null)
            {
                AppState.Instance.SetUser(response.User, response.Token ?? string.Empty);
                
                if (RememberMe)
                {
                    await SaveCredentials();
                }

                if (response.User.AssignedStations != null && response.User.AssignedStations.Count > 0)
                {
                    AppState.Instance.SetStation(response.User.AssignedStations[0]);
                    
                    // Navigate to AppShell by replacing the main page
                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    ErrorMessage = "No stations assigned to this user";
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

    private LoginResponse GetDummyLoginResponse()
    {
        // Simulate login validation with dummy users
        // Accept any of these credentials:
        var validCredentials = new Dictionary<string, string>
        {
            { "admin", "admin123" },
            { "cashier", "cashier123" },
            { "manager", "manager123" },
            { "demo", "demo" }
        };

        if (validCredentials.TryGetValue(Username.ToLower(), out var validPassword) && 
            Password == validPassword)
        {
            // Successful login - return dummy user data
            return new LoginResponse
            {
                Success = true,
                Token = $"dummy_token_{Guid.NewGuid():N}",
                Message = "Login successful",
                User = new UserInfo
                {
                    Id = 1,
                    Username = Username,
                    FullName = GetFullNameForUser(Username),
                    Role = GetRoleForUser(Username),
                    AssignedStations = GetDummyStations()
                }
            };
        }

        // Invalid credentials
        return new LoginResponse
        {
            Success = false,
            Message = "Invalid username or password.\n\nDemo accounts:\n• admin / admin123\n• cashier / cashier123\n• manager / manager123\n• demo / demo"
        };
    }

    private string GetFullNameForUser(string username)
    {
        return username.ToLower() switch
        {
            "admin" => "Admin User",
            "cashier" => "John Cashier",
            "manager" => "Jane Manager",
            "demo" => "Demo User",
            _ => username
        };
    }

    private string GetRoleForUser(string username)
    {
        return username.ToLower() switch
        {
            "admin" => "Administrator",
            "cashier" => "Cashier",
            "manager" => "Manager",
            "demo" => "Cashier",
            _ => "Cashier"
        };
    }

    private List<StationInfo> GetDummyStations()
    {
        return new List<StationInfo>
        {
            new StationInfo
            {
                Id = 1,
                Name = "Main Station",
                Location = "Downtown Kigali",
                Address = "KN 4 Ave, Kigali"
            },
            new StationInfo
            {
                Id = 2,
                Name = "Airport Station",
                Location = "Near Airport",
                Address = "Airport Road, Kigali"
            },
            new StationInfo
            {
                Id = 3,
                Name = "East Side Station",
                Location = "Remera",
                Address = "KG 9 Ave, Remera"
            }
        };
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
