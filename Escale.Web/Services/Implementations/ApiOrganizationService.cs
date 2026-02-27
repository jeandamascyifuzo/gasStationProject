using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;

namespace Escale.Web.Services.Implementations;

public class ApiOrganizationService : BaseApiService, IApiOrganizationService
{
    public ApiOrganizationService(HttpClient httpClient) : base(httpClient) { }

    public async Task<ApiResponse<List<OrganizationResponseDto>>> GetAllAsync()
        => await GetAsync<List<OrganizationResponseDto>>("/api/superadmin/organizations");

    public async Task<ApiResponse<OrganizationResponseDto>> GetByIdAsync(Guid id)
        => await GetAsync<OrganizationResponseDto>($"/api/superadmin/organizations/{id}");

    public async Task<ApiResponse<OrganizationResponseDto>> CreateAsync(CreateOrganizationRequestDto request)
        => await PostAsync<OrganizationResponseDto>("/api/superadmin/organizations", request);

    public async Task<ApiResponse<OrganizationResponseDto>> UpdateAsync(Guid id, UpdateOrganizationRequestDto request)
        => await PutAsync<OrganizationResponseDto>($"/api/superadmin/organizations/{id}", request);

    public async Task<ApiResponse> DeleteAsync(Guid id)
        => await base.DeleteAsync($"/api/superadmin/organizations/{id}");

    public async Task<ApiResponse<List<StationResponseDto>>> GetStationsAsync(Guid orgId)
        => await GetAsync<List<StationResponseDto>>($"/api/superadmin/organizations/{orgId}/stations");

    public async Task<ApiResponse<StationResponseDto>> CreateStationAsync(Guid orgId, CreateStationRequestDto request)
        => await PostAsync<StationResponseDto>($"/api/superadmin/organizations/{orgId}/stations", request);

    public async Task<ApiResponse> ConfigureEbmAsync(Guid orgId, EbmConfigRequestDto request)
        => await PutAsync($"/api/superadmin/organizations/{orgId}/settings/ebm", request);

    public async Task<ApiResponse<List<FuelTypeResponseDto>>> GetFuelTypesAsync(Guid orgId)
        => await GetAsync<List<FuelTypeResponseDto>>($"/api/superadmin/organizations/{orgId}/fueltypes");

    public async Task<ApiResponse<FuelTypeResponseDto>> CreateFuelTypeAsync(Guid orgId, CreateFuelTypeRequestDto request)
        => await PostAsync<FuelTypeResponseDto>($"/api/superadmin/organizations/{orgId}/fueltypes", request);

    public async Task<ApiResponse<FuelTypeResponseDto>> UpdateFuelTypeAsync(Guid orgId, Guid fuelTypeId, UpdateFuelTypeRequestDto request)
        => await PutAsync<FuelTypeResponseDto>($"/api/superadmin/organizations/{orgId}/fueltypes/{fuelTypeId}", request);

    public async Task<ApiResponse> DeleteFuelTypeAsync(Guid orgId, Guid fuelTypeId)
        => await base.DeleteAsync($"/api/superadmin/organizations/{orgId}/fueltypes/{fuelTypeId}");
}
