using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;

namespace Escale.Web.Services.Implementations;

public class ApiStationService : BaseApiService, IApiStationService
{
    public ApiStationService(HttpClient httpClient) : base(httpClient) { }

    public async Task<ApiResponse<List<StationResponseDto>>> GetAllAsync()
        => await GetAsync<List<StationResponseDto>>("/api/stations");

    public async Task<ApiResponse<StationDetailResponseDto>> GetByIdAsync(Guid id)
        => await GetAsync<StationDetailResponseDto>($"/api/stations/{id}");

    public async Task<ApiResponse<StationResponseDto>> CreateAsync(CreateStationRequestDto request)
        => await PostAsync<StationResponseDto>("/api/stations", request);

    public async Task<ApiResponse<StationResponseDto>> UpdateAsync(Guid id, UpdateStationRequestDto request)
        => await PutAsync<StationResponseDto>($"/api/stations/{id}", request);

    public async Task<ApiResponse> DeleteAsync(Guid id)
        => await base.DeleteAsync($"/api/stations/{id}");
}
