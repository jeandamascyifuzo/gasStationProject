using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escale.mobile.Services;

namespace Escale.mobile.ViewModels;

public partial class ChangePasswordViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    public ChangePasswordViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [ObservableProperty]
    private string currentPassword = string.Empty;

    [ObservableProperty]
    private string newPassword = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    private async Task Submit()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(CurrentPassword))
        {
            ErrorMessage = "Current password is required";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            ErrorMessage = "New password is required";
            return;
        }

        if (NewPassword.Length < 6)
        {
            ErrorMessage = "New password must be at least 6 characters";
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "New passwords do not match";
            return;
        }

        if (CurrentPassword == NewPassword)
        {
            ErrorMessage = "New password must be different from current password";
            return;
        }

        IsBusy = true;
        try
        {
            var (success, message) = await _apiService.ChangePasswordAsync(CurrentPassword, NewPassword);

            if (success)
            {
                await Shell.Current.DisplayAlert("Success", "Password changed successfully. Please log in again.", "OK");

                // Logout and go to login
                AppState.Instance.Logout();

                if (Application.Current != null)
                {
                    Application.Current.MainPage = new NavigationPage(
                        MauiProgram.Services.GetRequiredService<Views.LoginPage>());
                }
            }
            else
            {
                ErrorMessage = message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Shell.Current.Navigation.PopAsync();
    }
}
