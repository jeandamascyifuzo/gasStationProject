using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;

namespace Escale.Web.Services.Implementations;

public class ApiFuelTypeService : BaseApiService, IApiFuelTypeService
{
    public ApiFuelTypeService(HttpClient httpClient) : base(httpClient) { }

    public async Task<ApiResponse<List<FuelTypeResponseDto>>> GetAllAsync()
        => await GetAsync<List<FuelTypeResponseDto>>("/api/fueltypes");

    public async Task<ApiResponse<FuelTypeResponseDto>> GetByIdAsync(Guid id)
        => await GetAsync<FuelTypeResponseDto>($"/api/fueltypes/{id}");

    public async Task<ApiResponse<FuelTypeResponseDto>> CreateAsync(CreateFuelTypeRequestDto request)
        => await PostAsync<FuelTypeResponseDto>("/api/fueltypes", request);

    public async Task<ApiResponse<FuelTypeResponseDto>> UpdateAsync(Guid id, UpdateFuelTypeRequestDto request)
        => await PutAsync<FuelTypeResponseDto>($"/api/fueltypes/{id}", request);

    public async Task<ApiResponse> DeleteAsync(Guid id)
        => await base.DeleteAsync($"/api/fueltypes/{id}");
}
