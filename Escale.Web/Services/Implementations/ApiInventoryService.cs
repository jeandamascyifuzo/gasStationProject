using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;

namespace Escale.Web.Services.Implementations;

public class ApiInventoryService : BaseApiService, IApiInventoryService
{
    public ApiInventoryService(HttpClient httpClient) : base(httpClient) { }

    public async Task<ApiResponse<List<InventoryItemResponseDto>>> GetAllAsync(Guid? stationId = null)
    {
        var qs = stationId.HasValue ? $"?stationId={stationId}" : "";
        return await GetAsync<List<InventoryItemResponseDto>>($"/api/inventory{qs}");
    }

    public async Task<ApiResponse<List<RefillRecordResponseDto>>> GetRefillsAsync(int count = 10)
        => await GetAsync<List<RefillRecordResponseDto>>($"/api/inventory/refills?count={count}");

    public async Task<ApiResponse<RefillRecordResponseDto>> CreateRefillAsync(CreateRefillRequestDto request)
        => await PostAsync<RefillRecordResponseDto>("/api/inventory/refill", request);

    public async Task<ApiResponse> UpdateReorderLevelAsync(UpdateReorderLevelRequestDto request)
        => await PutAsync("/api/inventory/reorder-level", request);
}
