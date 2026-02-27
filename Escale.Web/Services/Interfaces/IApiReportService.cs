using Escale.Web.Models.Api;

namespace Escale.Web.Services.Interfaces;

public interface IApiReportService
{
    Task<ApiResponse<SalesReportDto>> GetSalesReportAsync(DateTime startDate, DateTime endDate, Guid? stationId = null);
    Task<ApiResponse<List<InventoryReportDto>>> GetInventoryReportAsync(Guid? stationId = null);
    Task<ApiResponse<List<EmployeeReportDto>>> GetEmployeeReportAsync(DateTime startDate, DateTime endDate);
    Task<ApiResponse<List<CustomerReportDto>>> GetCustomerReportAsync(DateTime startDate, DateTime endDate);
    Task<ApiResponse<FinancialReportDto>> GetFinancialReportAsync(DateTime startDate, DateTime endDate);
    Task<byte[]?> ExportTransactionsAsync(DateTime startDate, DateTime endDate, string format = "csv");
}
