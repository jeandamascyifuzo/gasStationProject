using Escale.API.DTOs.Reports;

namespace Escale.API.Services.Interfaces;

public interface IReportService
{
    Task<SalesReportDto> GetSalesReportAsync(DateTime startDate, DateTime endDate, Guid? stationId = null);
    Task<List<InventoryReportDto>> GetInventoryReportAsync(Guid? stationId = null);
    Task<List<EmployeeReportDto>> GetEmployeeReportAsync(DateTime startDate, DateTime endDate);
    Task<List<CustomerReportDto>> GetCustomerReportAsync(DateTime startDate, DateTime endDate);
    Task<FinancialReportDto> GetFinancialReportAsync(DateTime startDate, DateTime endDate);
    Task<byte[]> ExportTransactionsAsync(DateTime startDate, DateTime endDate, string format = "csv");
}
