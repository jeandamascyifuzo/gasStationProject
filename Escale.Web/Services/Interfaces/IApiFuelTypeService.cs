using Escale.Web.Models.Api;

namespace Escale.Web.Services.Interfaces;

public interface IApiFuelTypeService
{
    Task<ApiResponse<List<FuelTypeResponseDto>>> GetAllAsync();
    Task<ApiResponse<FuelTypeResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<FuelTypeResponseDto>> CreateAsync(CreateFuelTypeRequestDto request);
    Task<ApiResponse<FuelTypeResponseDto>> UpdateAsync(Guid id, UpdateFuelTypeRequestDto request);
    Task<ApiResponse> DeleteAsync(Guid id);
}
