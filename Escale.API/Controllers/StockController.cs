using Escale.API.DTOs.Common;
using Escale.API.DTOs.Stock;
using Escale.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<StockLevelDto>>>> GetStockLevels([FromQuery] Guid? stationId)
    {
        var result = await _stockService.GetStockLevelsAsync(stationId);
        return Ok(ApiResponse<List<StockLevelDto>>.SuccessResponse(result));
    }

    [HttpPost("refill")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse>> RecordRefill([FromBody] StockRefillRequest request)
    {
        await _stockService.RecordRefillAsync(request.StationId, request.FuelType, request.Quantity, request.UnitCost, request.SupplierName, request.InvoiceNumber, request.RefillDate);
        return Ok(ApiResponse.SuccessResponse("Refill recorded"));
    }
}

public class StockRefillRequest
{
    public Guid StationId { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? SupplierName { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime RefillDate { get; set; }
}
