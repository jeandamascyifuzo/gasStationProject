using Escale.API.DTOs.Common;
using Escale.API.DTOs.Settings;
using Escale.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<AppSettingsResponseDto>>> GetSettings()
    {
        var result = await _settingsService.GetSettingsAsync();
        return Ok(ApiResponse<AppSettingsResponseDto>.SuccessResponse(result));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<AppSettingsResponseDto>>> UpdateSettings([FromBody] UpdateSettingsRequestDto request)
    {
        var result = await _settingsService.UpdateSettingsAsync(request);
        return Ok(ApiResponse<AppSettingsResponseDto>.SuccessResponse(result, "Settings updated"));
    }

    [HttpGet("ebm/status")]
    public async Task<ActionResult<ApiResponse<EbmStatusDto>>> GetEbmStatus()
    {
        var result = await _settingsService.GetEbmStatusAsync();
        return Ok(ApiResponse<EbmStatusDto>.SuccessResponse(result));
    }

    [HttpPost("ebm/sync")]
    public async Task<ActionResult<ApiResponse<EbmStatusDto>>> SyncEbm()
    {
        var result = await _settingsService.SyncEbmAsync();
        return Ok(ApiResponse<EbmStatusDto>.SuccessResponse(result, "EBM synced"));
    }

    [HttpGet("ebm/config")]
    public async Task<ActionResult<ApiResponse<EbmConfigResponseDto>>> GetEbmConfig()
    {
        var result = await _settingsService.GetEbmConfigAsync();
        return Ok(ApiResponse<EbmConfigResponseDto>.SuccessResponse(result));
    }

    [HttpPost("ebm/test")]
    public async Task<ActionResult<ApiResponse<bool>>> TestEbmConnection()
    {
        var result = await _settingsService.TestEbmConnectionAsync();
        return Ok(result
            ? ApiResponse<bool>.SuccessResponse(true, "EBM connection successful!")
            : ApiResponse<bool>.ErrorResponse("EBM connection failed. Check configuration."));
    }
}
