using Escale.API.DTOs.Common;
using Escale.API.DTOs.Reports;
using Escale.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("sales")]
    public async Task<ActionResult<ApiResponse<SalesReportDto>>> GetSalesReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] Guid? stationId)
    {
        var result = await _reportService.GetSalesReportAsync(startDate, endDate, stationId);
        return Ok(ApiResponse<SalesReportDto>.SuccessResponse(result));
    }

    [HttpGet("inventory")]
    public async Task<ActionResult<ApiResponse<List<InventoryReportDto>>>> GetInventoryReport([FromQuery] Guid? stationId)
    {
        var result = await _reportService.GetInventoryReportAsync(stationId);
        return Ok(ApiResponse<List<InventoryReportDto>>.SuccessResponse(result));
    }

    [HttpGet("employees")]
    public async Task<ActionResult<ApiResponse<List<EmployeeReportDto>>>> GetEmployeeReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _reportService.GetEmployeeReportAsync(startDate, endDate);
        return Ok(ApiResponse<List<EmployeeReportDto>>.SuccessResponse(result));
    }

    [HttpGet("customers")]
    public async Task<ActionResult<ApiResponse<List<CustomerReportDto>>>> GetCustomerReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _reportService.GetCustomerReportAsync(startDate, endDate);
        return Ok(ApiResponse<List<CustomerReportDto>>.SuccessResponse(result));
    }

    [HttpGet("financial")]
    public async Task<ActionResult<ApiResponse<FinancialReportDto>>> GetFinancialReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var result = await _reportService.GetFinancialReportAsync(startDate, endDate);
        return Ok(ApiResponse<FinancialReportDto>.SuccessResponse(result));
    }

    [HttpGet("transactions/export")]
    public async Task<IActionResult> ExportTransactions([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string format = "csv")
    {
        var data = await _reportService.ExportTransactionsAsync(startDate, endDate, format);
        return File(data, "text/csv", $"transactions_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv");
    }
}
