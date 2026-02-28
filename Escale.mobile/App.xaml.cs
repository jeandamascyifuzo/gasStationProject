using Escale.mobile.Views;
using Escale.mobile.Services;

namespace Escale.mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Eagerly resolve AppState from DI so the static Instance is set
            MauiProgram.Services.GetRequiredService<AppState>();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // AppState.Instance is now safe to use
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
