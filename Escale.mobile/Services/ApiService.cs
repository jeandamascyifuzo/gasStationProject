using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Escale.mobile.Models;

namespace Escale.mobile.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private string _baseUrl;
    private string? _authToken;

    public ApiService()
    {
#if DEBUG
        // In development, bypass SSL certificate validation
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        
        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(60) // Increased timeout
        };
#else
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(60)
        };
#endif
        
        // Configure base URL based on platform
        _baseUrl = GetBaseUrl();
        
        System.Diagnostics.Debug.WriteLine($"API Base URL: {_baseUrl}");
    }

    private string GetBaseUrl()
    {
#if ANDROID
        // Android emulator uses 10.0.2.2 to access host machine's localhost
        return "https://10.0.2.2:7015/api";
#elif IOS
        // iOS simulator can use localhost
        return "https://localhost:7015/api";
#elif WINDOWS
        // Windows can use localhost
        return "https://localhost:7015/api";
#else
        // Default fallback
        return "https://localhost:7015/api";
#endif
    }

    public void SetAuthToken(string token)
    {
        _authToken = token;
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<(bool Success, string Message)> TestConnectionAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Testing connection to: {_baseUrl}");
            var response = await _httpClient.GetAsync($"{_baseUrl.Replace("/api", "")}/weatherforecast");
            
            if (response.IsSuccessStatusCode)
            {
                return (true, "API is reachable");
            }
            
            return (false, $"API returned: {response.StatusCode}");
        }
        catch (TaskCanceledException)
        {
            return (false, "Connection timeout. Make sure:\n1. The API is running\n2. Using correct URL\n3. Firewall allows connection");
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Connection failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<LoginResponse> LoginAsync(LoginModel login)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Attempting login to: {_baseUrl}/auth/login");
            System.Diagnostics.Debug.WriteLine($"Username: {login.Username}");
            
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/auth/login", login);
            
            System.Diagnostics.Debug.WriteLine($"Response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
                {
                    SetAuthToken(result.Token);
                }
                return result ?? new LoginResponse { Success = false, Message = "Invalid response" };
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return new LoginResponse 
            { 
                Success = false, 
                Message = $"Login failed: {response.StatusCode} - {errorContent}" 
            };
        }
        catch (TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("Login request timed out");
            return new LoginResponse 
            { 
                Success = false, 
                Message = $"Connection timeout. Please check:\n\n1. API is running (Escale.API project)\n2. API URL: {_baseUrl}\n3. Firewall/antivirus settings\n\nFor Android Emulator, make sure API runs on https://localhost:7015" 
            };
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Login network error: {ex.Message}");
            return new LoginResponse 
            { 
                Success = false, 
                Message = $"Cannot connect to API at {_baseUrl}\n\nError: {ex.Message}\n\nMake sure the Escale.API project is running." 
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
            return new LoginResponse 
            { 
                Success = false, 
                Message = $"Error: {ex.Message}" 
            };
        }
    }

    public async Task<DashboardSummary> GetDashboardSummaryAsync(int stationId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<DashboardSummary>(
                $"{_baseUrl}/dashboard/summary?stationId={stationId}");
            return response ?? new DashboardSummary();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching dashboard: {ex.Message}");
            return new DashboardSummary();
        }
    }

    public async Task<List<FuelTypeOption>> GetFuelTypesAsync(int stationId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<FuelTypeOption>>(
                $"{_baseUrl}/fueltypes?stationId={stationId}");
            return response ?? new List<FuelTypeOption>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching fuel types: {ex.Message}");
            return new List<FuelTypeOption>();
        }
    }

    public async Task<List<CustomerInfo>> SearchCustomersAsync(string searchTerm)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<CustomerInfo>>(
                $"{_baseUrl}/customers/search?term={Uri.EscapeDataString(searchTerm)}");
            return response ?? new List<CustomerInfo>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching customers: {ex.Message}");
            return new List<CustomerInfo>();
        }
    }

    public async Task<(bool Success, string Message, SaleModel? Sale)> SubmitSaleAsync(SaleModel sale)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/sales", sale);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SaleModel>();
                return (true, "Sale completed successfully", result);
            }
            
            var errorMessage = await response.Content.ReadAsStringAsync();
            return (false, $"Failed to submit sale: {errorMessage}", null);
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}", null);
        }
    }

    public async Task<List<Transaction>> GetTransactionsAsync(int stationId, DateTime date)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<Transaction>>(
                $"{_baseUrl}/transactions?stationId={stationId}&date={date:yyyy-MM-dd}");
            return response ?? new List<Transaction>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching transactions: {ex.Message}");
            return new List<Transaction>();
        }
    }

    public async Task<List<StockLevel>> GetStockLevelsAsync(int stationId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<StockLevel>>(
                $"{_baseUrl}/stock?stationId={stationId}");
            return response ?? new List<StockLevel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching stock levels: {ex.Message}");
            return new List<StockLevel>();
        }
    }

    public async Task<ShiftSummary> GetShiftSummaryAsync(int userId, int stationId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ShiftSummary>(
                $"{_baseUrl}/shifts/current?userId={userId}&stationId={stationId}");
            return response ?? new ShiftSummary();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching shift summary: {ex.Message}");
            return new ShiftSummary();
        }
    }

    public async Task<bool> ClockInOutAsync(int userId, int stationId, bool isClockIn)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/shifts/clock", 
                new { UserId = userId, StationId = stationId, IsClockIn = isClockIn });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clocking in/out: {ex.Message}");
            return false;
        }
    }
}
