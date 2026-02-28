using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Escale.mobile.Models;

namespace Escale.mobile.Services;

/// <summary>
/// Generic API response wrapper matching the API's ApiResponse<T> format.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}

/// <summary>
/// Sale creation request matching API's CreateSaleRequestDto.
/// </summary>
public class CreateSaleRequest
{
    public Guid StationId { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public Guid? FuelTypeId { get; set; }
    public decimal Liters { get; set; }
    public decimal PricePerLiter { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public SaleCustomerRequest? Customer { get; set; }
    public Guid? SubscriptionId { get; set; }
}

public class SaleCustomerRequest
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? PlateNumber { get; set; }
}

/// <summary>
/// Sale response matching API's SaleResponseDto.
/// </summary>
public class SaleResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public CompletedSale? Sale { get; set; }
}

public class CompletedSale
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string EBMCode { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal Liters { get; set; }
    public decimal PricePerLiter { get; set; }
    public decimal Subtotal { get; set; }
    public decimal VAT { get; set; }
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal? SubscriptionDeduction { get; set; }
    public decimal? SubscriptionRemainingBalance { get; set; }
}

/// <summary>
/// Dashboard summary matching API's DashboardSummaryDto.
/// </summary>
public class DashboardSummaryResponse
{
    public decimal TodaysSales { get; set; }
    public int TransactionCount { get; set; }
    public decimal AverageSale { get; set; }
    public List<StockAlertResponse> LowStockAlerts { get; set; } = new();
    public List<RecentTransactionResponse> RecentTransactions { get; set; } = new();
}

public class StockAlertResponse
{
    public string FuelType { get; set; } = string.Empty;
    public decimal CurrentLevel { get; set; }
    public decimal Capacity { get; set; }
    public decimal PercentageFull { get; set; }
}

public class RecentTransactionResponse
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal Liters { get; set; }
    public decimal Total { get; set; }
}

/// <summary>
/// Fuel type response matching API's FuelTypeResponseDto.
/// </summary>
public class FuelTypeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal PricePerLiter { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Transaction response matching API's TransactionResponseDto.
/// </summary>
public class TransactionResponse
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal Liters { get; set; }
    public decimal PricePerLiter { get; set; }
    public decimal Subtotal { get; set; }
    public decimal VAT { get; set; }
    public decimal Total { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public bool EBMSent { get; set; }
    public string? EBMCode { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public string StationName { get; set; } = string.Empty;
}

/// <summary>
/// Inventory item response matching API's InventoryItemResponseDto.
/// </summary>
public class InventoryItemResponse
{
    public Guid Id { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal CurrentLevel { get; set; }
    public decimal Capacity { get; set; }
    public decimal PercentageFull { get; set; }
    public decimal ReorderLevel { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastRefill { get; set; }
}

/// <summary>
/// Customer response matching API's CustomerResponseDto.
/// </summary>
public class CustomerResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentCredit { get; set; }
    public List<CarResponse> Cars { get; set; } = new();
}

public class CarResponse
{
    public Guid? Id { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string? Make { get; set; }
    public string? Model { get; set; }
    public bool IsActive { get; set; }
}

public class LookupCarRequest
{
    public string PlateNumber { get; set; } = string.Empty;
    public string PIN { get; set; } = string.Empty;
    public decimal? SaleAmount { get; set; }
}

public class SubscriptionCustomerLookupResponse
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool CustomerIsActive { get; set; }
    public Guid CarId { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public string? CarMake { get; set; }
    public string? CarModel { get; set; }
    public bool CarIsActive { get; set; }
    public Guid? ActiveSubscriptionId { get; set; }
    public decimal? RemainingBalance { get; set; }
    public DateTime? SubscriptionExpiryDate { get; set; }
    public bool HasSufficientBalance { get; set; }
    public string? ValidationError { get; set; }
}

/// <summary>
/// Shift response matching API's ShiftSummaryDto.
/// </summary>
public class ShiftSummaryResponse
{
    public Guid ShiftId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Duration { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalSales { get; set; }
    public decimal CashSales { get; set; }
    public decimal MobileMoneySales { get; set; }
    public decimal CardSales { get; set; }
    public decimal CreditSales { get; set; }
}

/// <summary>
/// Clock request matching API's ClockRequestDto.
/// </summary>
public class ClockRequest
{
    public Guid UserId { get; set; }
    public Guid StationId { get; set; }
    public bool IsClockIn { get; set; }
}

/// <summary>
/// Clock response matching API's ClockResponseDto.
/// </summary>
public class ClockResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Paged result matching API's PagedResult<T>.
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private string? _authToken;

    public ApiService()
    {
#if DEBUG
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
#else
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
#endif

        _baseUrl = GetBaseUrl();
        System.Diagnostics.Debug.WriteLine($"API Base URL: {_baseUrl}");
    }

    private static string GetBaseUrl()
    {
#if ANDROID
        return "https://10.0.2.2:7015/api";
#elif IOS
        return "https://localhost:7015/api";
#elif WINDOWS
        return "https://localhost:7015/api";
#else
        return "https://localhost:7015/api";
#endif
    }

    public void SetAuthToken(string token)
    {
        _authToken = token;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    // ==================== AUTH ====================

    public async Task<LoginResponse> LoginAsync(LoginModel login)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Logging in to: {_baseUrl}/auth/login");

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/auth/login", login);

            System.Diagnostics.Debug.WriteLine($"Login response status: {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Login response: {content}");

            // LoginResponseDto is returned directly (not wrapped in ApiResponse<T>)
            var result = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
            {
                SetAuthToken(result.Token);
            }

            return result ?? new LoginResponse { Success = false, Message = "Invalid response from server" };
        }
        catch (TaskCanceledException)
        {
            return new LoginResponse
            {
                Success = false,
                Message = $"Connection timeout. Please check:\n\n1. API is running (Escale.API project)\n2. API URL: {_baseUrl}\n3. Firewall/antivirus settings"
            };
        }
        catch (HttpRequestException ex)
        {
            return new LoginResponse
            {
                Success = false,
                Message = $"Cannot connect to API at {_baseUrl}\n\nError: {ex.Message}\n\nMake sure the Escale.API project is running."
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Login error: {ex}");
            return new LoginResponse { Success = false, Message = $"Error: {ex.Message}" };
        }
    }

    // ==================== DASHBOARD ====================

    public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(Guid? stationId)
    {
        try
        {
            var url = $"{_baseUrl}/dashboard/summary";
            if (stationId.HasValue)
                url += $"?stationId={stationId}";

            var response = await _httpClient.GetFromJsonAsync<ApiResponse<DashboardSummaryResponse>>(url);
            return response?.Data ?? new DashboardSummaryResponse();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching dashboard: {ex.Message}");
            return new DashboardSummaryResponse();
        }
    }

    // ==================== FUEL TYPES ====================

    public async Task<List<FuelTypeOption>> GetFuelTypesAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<FuelTypeResponse>>>(
                $"{_baseUrl}/fueltypes");

            if (response?.Data == null) return new List<FuelTypeOption>();

            return response.Data
                .Where(f => f.IsActive)
                .Select(f => new FuelTypeOption
                {
                    Id = f.Id,
                    Name = f.Name,
                    PricePerLiter = f.PricePerLiter,
                    Icon = f.Name.Contains("Diesel") ? "\u26fd" :
                           f.Name.Contains("Kerosene") ? "\ud83d\udca7" : "\u26fd",
                    BadgeColor = f.Name.Contains("Diesel") ? Colors.Orange :
                                 f.Name.Contains("Kerosene") ? Colors.Blue :
                                 f.Name.Contains("98") ? Colors.Green : Colors.Red
                }).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching fuel types: {ex.Message}");
            return new List<FuelTypeOption>();
        }
    }

    // ==================== SALES ====================

    public async Task<(bool Success, string Message, CompletedSale? Sale)> SubmitSaleAsync(SaleModel sale, Guid stationId)
    {
        try
        {
            var request = new CreateSaleRequest
            {
                StationId = stationId,
                FuelType = sale.FuelType,
                FuelTypeId = sale.FuelTypeId,
                Liters = sale.Liters ?? 0,
                PricePerLiter = sale.PricePerLiter,
                PaymentMethod = sale.PaymentMethod,
                SubscriptionId = sale.SubscriptionId,
                Customer = sale.Customer != null && !sale.Customer.IsWalkIn ? new SaleCustomerRequest
                {
                    Id = sale.Customer.Id,
                    Name = sale.Customer.Name,
                    PhoneNumber = sale.Customer.PhoneNumber,
                    PlateNumber = sale.Customer.PlateNumber
                } : null
            };

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/sales", request);
            var content = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"Sale response: {content}");

            // SaleResponseDto is returned directly (not wrapped in ApiResponse<T>)
            var result = JsonSerializer.Deserialize<SaleResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Success == true && result.Sale != null)
            {
                return (true, result.Message, result.Sale);
            }

            return (false, result?.Message ?? $"Sale failed: {response.StatusCode}", null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error submitting sale: {ex.Message}");
            return (false, $"Error: {ex.Message}", null);
        }
    }

    // ==================== TRANSACTIONS ====================

    public async Task<List<Transaction>> GetTransactionsAsync(Guid stationId, DateTime date)
    {
        try
        {
            var url = $"{_baseUrl}/transactions?StationId={stationId}&StartDate={date:yyyy-MM-dd}&EndDate={date:yyyy-MM-dd}&PageSize=50";
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<PagedResult<TransactionResponse>>>(url);

            if (response?.Data?.Items == null) return new List<Transaction>();

            return response.Data.Items.Select(t => new Transaction
            {
                Id = t.Id,
                ReceiptNumber = t.ReceiptNumber,
                TransactionDate = t.TransactionDate,
                FuelType = t.FuelType,
                Liters = t.Liters,
                Total = t.Total,
                PaymentMethod = t.PaymentMethod,
                CustomerName = t.CustomerName,
                EBMSent = t.EBMSent,
                EBMCode = t.EBMCode
            }).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching transactions: {ex.Message}");
            return new List<Transaction>();
        }
    }

    // ==================== STOCK / INVENTORY ====================

    public async Task<List<StockLevel>> GetStockLevelsAsync(Guid stationId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<InventoryItemResponse>>>(
                $"{_baseUrl}/inventory?stationId={stationId}");

            if (response?.Data == null) return new List<StockLevel>();

            return response.Data.Select(i => new StockLevel
            {
                FuelType = i.FuelType,
                CurrentLevel = i.CurrentLevel,
                Capacity = i.Capacity,
                LastUpdated = i.LastRefill ?? DateTime.Now
            }).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching stock levels: {ex.Message}");
            return new List<StockLevel>();
        }
    }

    // ==================== CUSTOMERS ====================

    public async Task<List<CustomerInfo>> SearchCustomersAsync(string searchTerm)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<CustomerResponse>>>(
                $"{_baseUrl}/customers/search?term={Uri.EscapeDataString(searchTerm)}");

            if (response?.Data == null) return new List<CustomerInfo>();

            return response.Data.Select(c => new CustomerInfo
            {
                Id = c.Id,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber ?? string.Empty,
                PlateNumber = c.Cars.FirstOrDefault()?.PlateNumber,
                VehicleModel = c.Cars.FirstOrDefault() != null
                    ? $"{c.Cars.First().Make} {c.Cars.First().Model}".Trim()
                    : null,
                CreditLimit = c.CreditLimit,
                CurrentCredit = c.CurrentCredit
            }).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching customers: {ex.Message}");
            return new List<CustomerInfo>();
        }
    }

    // ==================== SHIFTS ====================

    public async Task<ShiftSummary> GetShiftSummaryAsync(Guid userId, Guid stationId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<ShiftSummaryResponse>>(
                $"{_baseUrl}/shifts/summary?userId={userId}&stationId={stationId}");

            if (response?.Data == null) return new ShiftSummary();

            var d = response.Data;
            return new ShiftSummary
            {
                ShiftId = d.ShiftId,
                ShiftStart = d.StartTime,
                ShiftEnd = d.EndTime,
                Duration = d.Duration,
                TransactionCount = d.TransactionCount,
                TotalSales = d.TotalSales,
                CashSales = d.CashSales,
                MobileMoneySales = d.MobileMoneySales,
                CardSales = d.CardSales,
                CreditSales = d.CreditSales
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching shift summary: {ex.Message}");
            return new ShiftSummary();
        }
    }

    public async Task<(bool Success, string Message)> ClockInOutAsync(Guid userId, Guid stationId, bool isClockIn)
    {
        try
        {
            var request = new ClockRequest
            {
                UserId = userId,
                StationId = stationId,
                IsClockIn = isClockIn
            };

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/shifts/clock", request);
            var content = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ClockResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return (result?.Success ?? response.IsSuccessStatusCode,
                    result?.Message ?? (response.IsSuccessStatusCode ? "Success" : "Failed"));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error clocking in/out: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    // ==================== SUBSCRIPTIONS ====================

    public async Task<(bool Success, string? Error, SubscriptionCustomerLookupResponse? Data)> LookupSubscriptionCarAsync(string plateNumber, string pin, decimal? saleAmount = null)
    {
        try
        {
            var request = new LookupCarRequest
            {
                PlateNumber = plateNumber,
                PIN = pin,
                SaleAmount = saleAmount
            };

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/subscriptions/lookup", request);
            var content = await response.Content.ReadAsStringAsync();

            System.Diagnostics.Debug.WriteLine($"Subscription lookup response: {content}");

            var result = JsonSerializer.Deserialize<ApiResponse<SubscriptionCustomerLookupResponse>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Success == true && result.Data != null)
            {
                if (!string.IsNullOrEmpty(result.Data.ValidationError))
                {
                    return (false, result.Data.ValidationError, result.Data);
                }
                return (true, null, result.Data);
            }

            return (false, result?.Message ?? "Lookup failed", null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error looking up subscription car: {ex.Message}");
            return (false, $"Error: {ex.Message}", null);
        }
    }

    // ==================== CONNECTION TEST ====================

    public async Task<(bool Success, string Message)> TestConnectionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl.Replace("/api", "")}/weatherforecast");
            return response.IsSuccessStatusCode
                ? (true, "API is reachable")
                : (false, $"API returned: {response.StatusCode}");
        }
        catch (TaskCanceledException)
        {
            return (false, "Connection timeout.");
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
}
