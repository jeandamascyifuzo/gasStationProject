using Escale.API.DTOs.Common;
using Escale.API.DTOs.Sales;
using Escale.API.DTOs.Transactions;
using Escale.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;

    public SalesController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    [HttpPost]
    public async Task<ActionResult<SaleResponseDto>> CreateSale([FromBody] CreateSaleRequestDto request)
    {
        var result = await _saleService.CreateSaleAsync(request);
        return Ok(result);
    }

    [HttpGet("recent")]
    public async Task<ActionResult<ApiResponse<List<TransactionResponseDto>>>> GetRecentSales([FromQuery] Guid? stationId, [FromQuery] int count = 10)
    {
        var result = await _saleService.GetRecentSalesAsync(stationId, count);
        return Ok(ApiResponse<List<TransactionResponseDto>>.SuccessResponse(result));
    }
}
