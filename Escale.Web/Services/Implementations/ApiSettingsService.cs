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

    public async Task<ApiResponse<EbmConfigResponseDto>> GetEbmConfigAsync()
        => await GetAsync<EbmConfigResponseDto>("/api/settings/ebm/config");

    public async Task<ApiResponse<bool>> TestEbmConnectionAsync()
        => await PostAsync<bool>("/api/settings/ebm/test");

    public async Task<ApiResponse<string>> UploadLogoAsync(IFormFile file)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            var response = await HttpClient.PostAsync("/api/settings/logo", content);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<string>>(json,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result ?? new ApiResponse<string> { Success = true };
            }

            return new ApiResponse<string> { Success = false, Message = $"Upload failed: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<string> { Success = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<string>> GetLogoUrlAsync()
        => await GetAsync<string>("/api/settings/logo");

    public async Task<ApiResponse<List<PaymentMethodSettingDto>>> GetPaymentMethodsAsync()
        => await GetAsync<List<PaymentMethodSettingDto>>("/api/settings/payment-methods");

    public async Task<ApiResponse<PaymentMethodSettingDto>> UpdatePaymentMethodAsync(Guid id, UpdatePaymentMethodDto request)
        => await PutAsync<PaymentMethodSettingDto>($"/api/settings/payment-methods/{id}", request);
}
