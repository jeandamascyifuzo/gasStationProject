using Escale.Web.Configuration;
using Escale.Web.Filters;
using Escale.Web.Handlers;
using Escale.Web.Services.Implementations;
using Escale.Web.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>() ?? new ApiSettings();

// Core services
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<RequireAuthAttribute>();
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// Auth handler (transient - one per request)
builder.Services.AddTransient<AuthenticatedHttpClientHandler>();

// Auth service - separate HttpClient without auth handler (used for login itself)
builder.Services.AddHttpClient<IApiAuthService, ApiAuthService>(client =>
{
    client.BaseAddress = new Uri(apiSettings.BaseUrl.Replace("/api", ""));
    client.Timeout = TimeSpan.FromSeconds(apiSettings.TimeoutSeconds);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
});

// Register all domain services with authenticated HttpClient
void RegisterApiService<TInterface, TImplementation>()
    where TInterface : class
    where TImplementation : class, TInterface
{
    builder.Services.AddHttpClient<TInterface, TImplementation>(client =>
    {
        client.BaseAddress = new Uri(apiSettings.BaseUrl.Replace("/api", ""));
        client.Timeout = TimeSpan.FromSeconds(apiSettings.TimeoutSeconds);
    })
    .AddHttpMessageHandler<AuthenticatedHttpClientHandler>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
    });
}

RegisterApiService<IApiDashboardService, ApiDashboardService>();
RegisterApiService<IApiStationService, ApiStationService>();
RegisterApiService<IApiFuelTypeService, ApiFuelTypeService>();
RegisterApiService<IApiUserService, ApiUserService>();
RegisterApiService<IApiCustomerService, ApiCustomerService>();
RegisterApiService<IApiTransactionService, ApiTransactionService>();
RegisterApiService<IApiInventoryService, ApiInventoryService>();
RegisterApiService<IApiReportService, ApiReportService>();
RegisterApiService<IApiSettingsService, ApiSettingsService>();
RegisterApiService<IApiOrganizationService, ApiOrganizationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
