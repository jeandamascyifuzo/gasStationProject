using Escale.API.DTOs.Common;
using Escale.API.DTOs.Shifts;
using Escale.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;

    public ShiftsController(IShiftService shiftService)
    {
        _shiftService = shiftService;
    }

    [HttpGet("current")]
    public async Task<ActionResult<ApiResponse<ShiftResponseDto>>> GetCurrentShift([FromQuery] Guid userId, [FromQuery] Guid stationId)
    {
        var result = await _shiftService.GetCurrentShiftAsync(userId, stationId);
        return Ok(ApiResponse<ShiftResponseDto?>.SuccessResponse(result));
    }

    [HttpPost("clock")]
    public async Task<ActionResult<ClockResponseDto>> Clock([FromBody] ClockRequestDto request)
    {
        var result = await _shiftService.ClockAsync(request);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<ShiftSummaryDto>>> GetShiftSummary([FromQuery] Guid userId, [FromQuery] Guid stationId)
    {
        var result = await _shiftService.GetShiftSummaryAsync(userId, stationId);
        return Ok(ApiResponse<ShiftSummaryDto?>.SuccessResponse(result));
    }
}
