using Escale.mobile.Views;
using Escale.mobile.Services;

namespace Escale.mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Check if user is logged in
            var isLoggedIn = AppState.Instance.IsLoggedIn;
            
            if (isLoggedIn)
            {
                return new Window(new AppShell());
            }
            else
            {
                return new Window(new NavigationPage(
                    MauiProgram.Services.GetRequiredService<LoginPage>()));
            }
        }
    }
}