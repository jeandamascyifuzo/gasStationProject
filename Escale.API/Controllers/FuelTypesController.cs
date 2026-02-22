using Escale.API.DTOs.Common;
using Escale.API.DTOs.FuelTypes;
using Escale.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FuelTypesController : ControllerBase
{
    private readonly IFuelTypeService _fuelTypeService;

    public FuelTypesController(IFuelTypeService fuelTypeService)
    {
        _fuelTypeService = fuelTypeService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<FuelTypeResponseDto>>>> GetFuelTypes()
    {
        var result = await _fuelTypeService.GetFuelTypesAsync();
        return Ok(ApiResponse<List<FuelTypeResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<FuelTypeResponseDto>>> GetFuelType(Guid id)
    {
        var result = await _fuelTypeService.GetFuelTypeByIdAsync(id);
        return Ok(ApiResponse<FuelTypeResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<FuelTypeResponseDto>>> CreateFuelType([FromBody] CreateFuelTypeRequestDto request)
    {
        var result = await _fuelTypeService.CreateFuelTypeAsync(request);
        return Ok(ApiResponse<FuelTypeResponseDto>.SuccessResponse(result, "Fuel type created"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<FuelTypeResponseDto>>> UpdateFuelType(Guid id, [FromBody] UpdateFuelTypeRequestDto request)
    {
        var result = await _fuelTypeService.UpdateFuelTypeAsync(id, request);
        return Ok(ApiResponse<FuelTypeResponseDto>.SuccessResponse(result, "Fuel type updated"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> DeleteFuelType(Guid id)
    {
        await _fuelTypeService.DeleteFuelTypeAsync(id);
        return Ok(ApiResponse.SuccessResponse("Fuel type deleted"));
    }
}
