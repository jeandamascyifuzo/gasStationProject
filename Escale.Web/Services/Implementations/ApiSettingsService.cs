using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;

namespace Escale.Web.Services.Implementations;

public class ApiSettingsService : BaseApiService, IApiSettingsService
{
    public ApiSettingsService(HttpClient httpClient) : base(httpClient) { }

    public async Task<ApiResponse<AppSettingsResponseDto>> GetSettingsAsync()
        => await GetAsync<AppSettingsResponseDto>("/api/settings");

    public async Task<ApiResponse<AppSettingsResponseDto>> UpdateSettingsAsync(UpdateSettingsRequestDto request)
        => await PutAsync<AppSettingsResponseDto>("/api/settings", request);

    public async Task<ApiResponse<EbmStatusDto>> GetEbmStatusAsync()
        => await GetAsync<EbmStatusDto>("/api/settings/ebm/status");

    public async Task<ApiResponse<EbmStatusDto>> SyncEbmAsync()
        => await PostAsync<EbmStatusDto>("/api/settings/ebm/sync");
}
