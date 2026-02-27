using Escale.Web.Models.Api;

namespace Escale.Web.Services.Interfaces;

public interface IApiSettingsService
{
    Task<ApiResponse<AppSettingsResponseDto>> GetSettingsAsync();
    Task<ApiResponse<AppSettingsResponseDto>> UpdateSettingsAsync(UpdateSettingsRequestDto request);
    Task<ApiResponse<EbmStatusDto>> GetEbmStatusAsync();
    Task<ApiResponse<EbmStatusDto>> SyncEbmAsync();
}
