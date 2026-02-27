using Escale.API.DTOs.Common;
using Escale.API.DTOs.FuelTypes;
using Escale.API.DTOs.Organizations;
using Escale.API.DTOs.Settings;
using Escale.API.DTOs.Stations;
using Escale.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class SuperAdminController : ControllerBase
{
    private readonly IOrganizationService _organizationService;

    public SuperAdminController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet("organizations")]
    public async Task<ActionResult<ApiResponse<List<OrganizationResponseDto>>>> GetOrganizations()
    {
        var result = await _organizationService.GetAllOrganizationsAsync();
        return Ok(ApiResponse<List<OrganizationResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("organizations/{id}")]
    public async Task<ActionResult<ApiResponse<OrganizationResponseDto>>> GetOrganization(Guid id)
    {
        var result = await _organizationService.GetOrganizationByIdAsync(id);
        return Ok(ApiResponse<OrganizationResponseDto>.SuccessResponse(result));
    }

    [HttpPost("organizations")]
    public async Task<ActionResult<ApiResponse<OrganizationResponseDto>>> CreateOrganization([FromBody] CreateOrganizationRequestDto request)
    {
        var result = await _organizationService.CreateOrganizationAsync(request);
        return Ok(ApiResponse<OrganizationResponseDto>.SuccessResponse(result, "Organization created"));
    }

    [HttpPut("organizations/{id}")]
    public async Task<ActionResult<ApiResponse<OrganizationResponseDto>>> UpdateOrganization(Guid id, [FromBody] UpdateOrganizationRequestDto request)
    {
        var result = await _organizationService.UpdateOrganizationAsync(id, request);
        return Ok(ApiResponse<OrganizationResponseDto>.SuccessResponse(result, "Organization updated"));
    }

    [HttpDelete("organizations/{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteOrganization(Guid id)
    {
        await _organizationService.DeleteOrganizationAsync(id);
        return Ok(ApiResponse.SuccessResponse("Organization deleted"));
    }

    [HttpGet("organizations/{orgId}/stations")]
    public async Task<ActionResult<ApiResponse<List<StationResponseDto>>>> GetOrganizationStations(Guid orgId)
    {
        var result = await _organizationService.GetOrganizationStationsAsync(orgId);
        return Ok(ApiResponse<List<StationResponseDto>>.SuccessResponse(result));
    }

    [HttpPost("organizations/{orgId}/stations")]
    public async Task<ActionResult<ApiResponse<StationResponseDto>>> CreateOrganizationStation(Guid orgId, [FromBody] CreateStationRequestDto request)
    {
        var result = await _organizationService.CreateOrganizationStationAsync(orgId, request);
        return Ok(ApiResponse<StationResponseDto>.SuccessResponse(result, "Station created"));
    }

    [HttpPut("organizations/{orgId}/settings/ebm")]
    public async Task<ActionResult<ApiResponse>> ConfigureEbm(Guid orgId, [FromBody] EbmConfigRequestDto request)
    {
        await _organizationService.ConfigureEbmAsync(orgId, request);
        return Ok(ApiResponse.SuccessResponse("EBM configuration updated"));
    }

    [HttpGet("organizations/{orgId}/fueltypes")]
    public async Task<ActionResult<ApiResponse<List<FuelTypeResponseDto>>>> GetOrganizationFuelTypes(Guid orgId)
    {
        var result = await _organizationService.GetOrganizationFuelTypesAsync(orgId);
        return Ok(ApiResponse<List<FuelTypeResponseDto>>.SuccessResponse(result));
    }

    [HttpPost("organizations/{orgId}/fueltypes")]
    public async Task<ActionResult<ApiResponse<FuelTypeResponseDto>>> CreateOrganizationFuelType(Guid orgId, [FromBody] CreateFuelTypeRequestDto request)
    {
        var result = await _organizationService.CreateOrganizationFuelTypeAsync(orgId, request);
        return Ok(ApiResponse<FuelTypeResponseDto>.SuccessResponse(result, "Fuel type created"));
    }

    [HttpPut("organizations/{orgId}/fueltypes/{fuelTypeId}")]
    public async Task<ActionResult<ApiResponse<FuelTypeResponseDto>>> UpdateOrganizationFuelType(Guid orgId, Guid fuelTypeId, [FromBody] UpdateFuelTypeRequestDto request)
    {
        var result = await _organizationService.UpdateOrganizationFuelTypeAsync(orgId, fuelTypeId, request);
        return Ok(ApiResponse<FuelTypeResponseDto>.SuccessResponse(result, "Fuel type updated"));
    }

    [HttpDelete("organizations/{orgId}/fueltypes/{fuelTypeId}")]
    public async Task<ActionResult<ApiResponse>> DeleteOrganizationFuelType(Guid orgId, Guid fuelTypeId)
    {
        await _organizationService.DeleteOrganizationFuelTypeAsync(orgId, fuelTypeId);
        return Ok(ApiResponse.SuccessResponse("Fuel type deleted"));
    }
}
