using Escale.API.DTOs.Inventory;

namespace Escale.API.Services.Interfaces;

public interface IInventoryService
{
    Task<List<InventoryItemResponseDto>> GetInventoryAsync(Guid? stationId = null);
    Task<List<RefillRecordResponseDto>> GetRefillsAsync(int count = 20);
    Task<RefillRecordResponseDto> RecordRefillAsync(CreateRefillRequestDto request);
    Task UpdateReorderLevelAsync(UpdateReorderLevelRequestDto request);
}
