using Escale.Web.Models.Api;

namespace Escale.Web.Services.Interfaces;

public interface IApiStationService
{
    Task<ApiResponse<List<StationResponseDto>>> GetAllAsync();
    Task<ApiResponse<StationDetailResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<StationResponseDto>> CreateAsync(CreateStationRequestDto request);
    Task<ApiResponse<StationResponseDto>> UpdateAsync(Guid id, UpdateStationRequestDto request);
    Task<ApiResponse> DeleteAsync(Guid id);
}
