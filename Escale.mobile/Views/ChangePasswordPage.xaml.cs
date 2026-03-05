using Escale.mobile.Helpers;
using Escale.mobile.ViewModels;

namespace Escale.mobile.Views;

public partial class ChangePasswordPage : ContentPage
{
    public ChangePasswordPage(ChangePasswordViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnTogglePasswordVisibility(object sender, EventArgs e)
    {
        if (sender is not ImageButton btn) return;

        // Find the Entry sibling in the same Grid
        if (btn.Parent is Grid grid)
        {
            var entry = grid.Children.OfType<Entry>().FirstOrDefault();
            if (entry != null)
            {
                entry.IsPassword = !entry.IsPassword;
                btn.Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = entry.IsPassword ? IconFont.EyeOff : IconFont.Eye,
                    Color = Color.FromArgb("#94A3B8"),
                    Size = 18
                };
            }
        }
    }
}
