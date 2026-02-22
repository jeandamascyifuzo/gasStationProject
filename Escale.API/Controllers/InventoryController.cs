using Escale.API.DTOs.Common;
using Escale.API.DTOs.Inventory;
using Escale.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<InventoryItemResponseDto>>>> GetInventory([FromQuery] Guid? stationId)
    {
        var result = await _inventoryService.GetInventoryAsync(stationId);
        return Ok(ApiResponse<List<InventoryItemResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("refills")]
    public async Task<ActionResult<ApiResponse<List<RefillRecordResponseDto>>>> GetRefills([FromQuery] int count = 20)
    {
        var result = await _inventoryService.GetRefillsAsync(count);
        return Ok(ApiResponse<List<RefillRecordResponseDto>>.SuccessResponse(result));
    }

    [HttpPost("refill")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<RefillRecordResponseDto>>> RecordRefill([FromBody] CreateRefillRequestDto request)
    {
        var result = await _inventoryService.RecordRefillAsync(request);
        return Ok(ApiResponse<RefillRecordResponseDto>.SuccessResponse(result, "Refill recorded"));
    }

    [HttpPut("reorder-level")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse>> UpdateReorderLevel([FromBody] UpdateReorderLevelRequestDto request)
    {
        await _inventoryService.UpdateReorderLevelAsync(request);
        return Ok(ApiResponse.SuccessResponse("Reorder level updated"));
    }
}
