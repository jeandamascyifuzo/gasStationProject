using Escale.mobile.Views;
using Escale.mobile.Services;

namespace Escale.mobile
{
    public partial class App : Application
    {
        // Pre-created shell — avoids ~10s creation delay after login
        private static AppShell? _cachedShell;

        public App()
        {
            InitializeComponent();

            // Eagerly resolve AppState from DI so the static Instance is set
            MauiProgram.Services.GetRequiredService<AppState>();

            // Pre-warm HTTPS connection in background (SSL handshake is slow on first request)
            var apiService = MauiProgram.Services.GetRequiredService<ApiService>();
            _ = apiService.PreWarmConnectionAsync();

            // Pre-create AppShell in background while user sees login page
            if (!AppState.Instance.IsLoggedIn)
            {
                _ = Task.Run(() =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        _cachedShell = new AppShell();
                        System.Diagnostics.Debug.WriteLine($"[App] AppShell pre-created in {sw.ElapsedMilliseconds}ms");
                    });
                });
            }
        }

        /// <summary>
        /// Gets the pre-created AppShell or creates a new one.
        /// </summary>
        public static AppShell GetOrCreateShell()
        {
            var shell = _cachedShell ?? new AppShell();
            _cachedShell = null; // consume it
            return shell;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            if (AppState.Instance.IsLoggedIn)
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
