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
    private readonly IOrganizationService _organizationService;
    private readonly ICurrentUserService _currentUser;

    public SettingsController(ISettingsService settingsService, IOrganizationService organizationService, ICurrentUserService currentUser)
    {
        _settingsService = settingsService;
        _organizationService = organizationService;
        _currentUser = currentUser;
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

    [HttpPost("logo")]
    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<string>>> UploadLogo(IFormFile file)
    {
        var orgId = _currentUser.OrganizationId;
        if (orgId == null)
            return BadRequest(ApiResponse<string>.ErrorResponse("Organization not found"));

        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.ErrorResponse("No file uploaded"));

        if (file.Length > 2 * 1024 * 1024)
            return BadRequest(ApiResponse<string>.ErrorResponse("File size must be less than 2MB"));

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
            return BadRequest(ApiResponse<string>.ErrorResponse("Only image files (jpg, png, gif, webp) are allowed"));

        using var stream = file.OpenReadStream();
        var logoUrl = await _organizationService.UploadLogoAsync(orgId.Value, stream, file.FileName);
        return Ok(ApiResponse<string>.SuccessResponse(logoUrl, "Logo uploaded successfully"));
    }

    [HttpGet("logo")]
    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    public async Task<ActionResult<ApiResponse<string>>> GetLogoUrl()
    {
        var orgId = _currentUser.OrganizationId;
        if (orgId == null)
            return BadRequest(ApiResponse<string>.ErrorResponse("Organization not found"));

        var org = await _organizationService.GetOrganizationByIdAsync(orgId.Value);
        return Ok(ApiResponse<string>.SuccessResponse(org.LogoUrl ?? ""));
    }
}
