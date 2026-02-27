using Escale.Web.Models.Api;

namespace Escale.Web.Services.Interfaces;

public interface IApiInventoryService
{
    Task<ApiResponse<List<InventoryItemResponseDto>>> GetAllAsync(Guid? stationId = null);
    Task<ApiResponse<List<RefillRecordResponseDto>>> GetRefillsAsync(int count = 10);
    Task<ApiResponse<RefillRecordResponseDto>> CreateRefillAsync(CreateRefillRequestDto request);
    Task<ApiResponse> UpdateReorderLevelAsync(UpdateReorderLevelRequestDto request);
}
