using Escale.API.DTOs.Settings;

namespace Escale.API.Services.Interfaces;

public interface ISettingsService
{
    Task<AppSettingsResponseDto> GetSettingsAsync();
    Task<AppSettingsResponseDto> UpdateSettingsAsync(UpdateSettingsRequestDto request);
    Task<EbmStatusDto> GetEbmStatusAsync();
    Task<EbmStatusDto> SyncEbmAsync();
}
