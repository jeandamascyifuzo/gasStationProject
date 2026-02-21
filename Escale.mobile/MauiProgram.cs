using Microsoft.Extensions.Logging;
using Escale.mobile.Services;
using Escale.mobile.ViewModels;
using Escale.mobile.Views;

namespace Escale.mobile
{
    public static class MauiProgram
    {
        public static IServiceProvider Services { get; private set; } = null!;

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .ConfigureMauiHandlers(handlers =>
                {
#if ANDROID
                    // Customize Android tab bar appearance
                    handlers.AddHandler(typeof(Shell), typeof(Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer));
#endif
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Register Services
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<AppState>();

            // Register ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<StationSelectionViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<NewSaleViewModel>();
            builder.Services.AddTransient<CustomerSearchViewModel>();
            builder.Services.AddTransient<CustomerInfoViewModel>();
            builder.Services.AddTransient<SalePreviewViewModel>();
            builder.Services.AddTransient<SaleCompleteViewModel>();
            builder.Services.AddTransient<StockViewModel>();
            builder.Services.AddTransient<TransactionsViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();

            // Register Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<StationSelectionPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<NewSalePage>();
            builder.Services.AddTransient<CustomerSearchPage>();
            builder.Services.AddTransient<CustomerInfoPage>();
            builder.Services.AddTransient<SalePreviewPage>();
            builder.Services.AddTransient<SaleCompletePage>();
            builder.Services.AddTransient<StockPage>();
            builder.Services.AddTransient<TransactionsPage>();
            builder.Services.AddTransient<ProfilePage>();

            var app = builder.Build();
            Services = app.Services;
            return app;
        }
    }
}
