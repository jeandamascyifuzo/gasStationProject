using Escale.Web.Models.Api;

namespace Escale.Web.Services.Interfaces;

public interface IApiOrganizationService
{
    Task<ApiResponse<List<OrganizationResponseDto>>> GetAllAsync();
    Task<ApiResponse<OrganizationResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<OrganizationResponseDto>> CreateAsync(CreateOrganizationRequestDto request);
    Task<ApiResponse<OrganizationResponseDto>> UpdateAsync(Guid id, UpdateOrganizationRequestDto request);
    Task<ApiResponse> DeleteAsync(Guid id);
    Task<ApiResponse<List<StationResponseDto>>> GetStationsAsync(Guid orgId);
    Task<ApiResponse<StationResponseDto>> CreateStationAsync(Guid orgId, CreateStationRequestDto request);
    Task<ApiResponse> ConfigureEbmAsync(Guid orgId, EbmConfigRequestDto request);
    Task<ApiResponse<List<FuelTypeResponseDto>>> GetFuelTypesAsync(Guid orgId);
    Task<ApiResponse<FuelTypeResponseDto>> CreateFuelTypeAsync(Guid orgId, CreateFuelTypeRequestDto request);
    Task<ApiResponse<FuelTypeResponseDto>> UpdateFuelTypeAsync(Guid orgId, Guid fuelTypeId, UpdateFuelTypeRequestDto request);
    Task<ApiResponse> DeleteFuelTypeAsync(Guid orgId, Guid fuelTypeId);
}
