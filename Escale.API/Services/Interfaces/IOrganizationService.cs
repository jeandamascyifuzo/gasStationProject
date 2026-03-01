using Escale.API.DTOs.FuelTypes;
using Escale.API.DTOs.Organizations;
using Escale.API.DTOs.Settings;
using Escale.API.DTOs.Stations;

namespace Escale.API.Services.Interfaces;

public interface IOrganizationService
{
    Task<List<OrganizationResponseDto>> GetAllOrganizationsAsync();
    Task<OrganizationResponseDto> GetOrganizationByIdAsync(Guid id);
    Task<OrganizationResponseDto> CreateOrganizationAsync(CreateOrganizationRequestDto request);
    Task<OrganizationResponseDto> UpdateOrganizationAsync(Guid id, UpdateOrganizationRequestDto request);
    Task DeleteOrganizationAsync(Guid id);
    Task<List<StationResponseDto>> GetOrganizationStationsAsync(Guid orgId);
    Task<StationResponseDto> CreateOrganizationStationAsync(Guid orgId, CreateStationRequestDto request);
    Task ConfigureEbmAsync(Guid orgId, EbmConfigRequestDto request);
    Task<EbmConfigResponseDto> GetEbmConfigAsync(Guid orgId);
    Task<List<FuelTypeResponseDto>> GetOrganizationFuelTypesAsync(Guid orgId);
    Task<FuelTypeResponseDto> CreateOrganizationFuelTypeAsync(Guid orgId, CreateFuelTypeRequestDto request);
    Task<FuelTypeResponseDto> UpdateOrganizationFuelTypeAsync(Guid orgId, Guid fuelTypeId, UpdateFuelTypeRequestDto request);
    Task DeleteOrganizationFuelTypeAsync(Guid orgId, Guid fuelTypeId);
}
