using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;

namespace Escale.Web.Services.Implementations;

public class ApiReportService : BaseApiService, IApiReportService
{
    public ApiReportService(HttpClient httpClient) : base(httpClient) { }

    public async Task<ApiResponse<SalesReportDto>> GetSalesReportAsync(DateTime startDate, DateTime endDate, Guid? stationId = null)
    {
        var query = $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        if (stationId.HasValue) query += $"&stationId={stationId}";
        return await GetAsync<SalesReportDto>($"/api/reports/sales{query}");
    }

    public async Task<ApiResponse<List<InventoryReportDto>>> GetInventoryReportAsync(Guid? stationId = null)
    {
        var qs = stationId.HasValue ? $"?stationId={stationId}" : "";
        return await GetAsync<List<InventoryReportDto>>($"/api/reports/inventory{qs}");
    }

    public async Task<ApiResponse<List<EmployeeReportDto>>> GetEmployeeReportAsync(DateTime startDate, DateTime endDate)
        => await GetAsync<List<EmployeeReportDto>>($"/api/reports/employees?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

    public async Task<ApiResponse<List<CustomerReportDto>>> GetCustomerReportAsync(DateTime startDate, DateTime endDate)
        => await GetAsync<List<CustomerReportDto>>($"/api/reports/customers?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

    public async Task<ApiResponse<FinancialReportDto>> GetFinancialReportAsync(DateTime startDate, DateTime endDate)
        => await GetAsync<FinancialReportDto>($"/api/reports/financial?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

    public async Task<byte[]?> ExportTransactionsAsync(DateTime startDate, DateTime endDate, string format = "csv")
        => await GetBytesAsync($"/api/reports/transactions/export?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&format={format}");
}
