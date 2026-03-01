using Escale.API.DTOs.Common;
using Escale.API.DTOs.Dashboard;
using Escale.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> GetSummary([FromQuery] Guid? stationId, [FromQuery] DateTime? date)
    {
        var result = await _dashboardService.GetSummaryAsync(stationId, date);
        return Ok(ApiResponse<DashboardSummaryDto>.SuccessResponse(result));
    }

    [HttpGet("station-performance")]
    public async Task<ActionResult<ApiResponse<List<StationPerformanceDto>>>> GetStationPerformance(
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int top = 5)
    {
        var result = await _dashboardService.GetStationPerformanceAsync(startDate, endDate, top);
        return Ok(ApiResponse<List<StationPerformanceDto>>.SuccessResponse(result));
    }
}
