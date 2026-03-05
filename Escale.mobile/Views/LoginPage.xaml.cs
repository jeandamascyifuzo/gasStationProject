using Escale.mobile.Helpers;
using Escale.mobile.ViewModels;

namespace Escale.mobile.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnTogglePasswordVisibility(object sender, EventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
        if (sender is ImageButton btn)
        {
            btn.Source = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = PasswordEntry.IsPassword ? IconFont.EyeOff : IconFont.Eye,
                Color = Color.FromArgb("#94A3B8"),
                Size = 20
            };
        }
    }
}
