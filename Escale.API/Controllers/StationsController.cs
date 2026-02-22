using Escale.API.DTOs.Common;
using Escale.API.DTOs.Stations;
using Escale.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StationsController : ControllerBase
{
    private readonly IStationService _stationService;

    public StationsController(IStationService stationService)
    {
        _stationService = stationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<StationResponseDto>>>> GetStations()
    {
        var result = await _stationService.GetStationsAsync();
        return Ok(ApiResponse<List<StationResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<StationDetailResponseDto>>> GetStation(Guid id)
    {
        var result = await _stationService.GetStationByIdAsync(id);
        return Ok(ApiResponse<StationDetailResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<StationResponseDto>>> CreateStation([FromBody] CreateStationRequestDto request)
    {
        var result = await _stationService.CreateStationAsync(request);
        return Ok(ApiResponse<StationResponseDto>.SuccessResponse(result, "Station created"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<StationResponseDto>>> UpdateStation(Guid id, [FromBody] UpdateStationRequestDto request)
    {
        var result = await _stationService.UpdateStationAsync(id, request);
        return Ok(ApiResponse<StationResponseDto>.SuccessResponse(result, "Station updated"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> DeleteStation(Guid id)
    {
        await _stationService.DeleteStationAsync(id);
        return Ok(ApiResponse.SuccessResponse("Station deleted"));
    }
}
