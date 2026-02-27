using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;

namespace Escale.Web.Services.Implementations;

public class ApiUserService : BaseApiService, IApiUserService
{
    public ApiUserService(HttpClient httpClient) : base(httpClient) { }

    public async Task<ApiResponse<PagedResult<UserResponseDto>>> GetAllAsync(int page = 1, int pageSize = 20, string? searchTerm = null)
    {
        var query = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(searchTerm)) query += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
        return await GetAsync<PagedResult<UserResponseDto>>($"/api/users{query}");
    }

    public async Task<ApiResponse<UserResponseDto>> GetByIdAsync(Guid id)
        => await GetAsync<UserResponseDto>($"/api/users/{id}");

    public async Task<ApiResponse<UserResponseDto>> CreateAsync(CreateUserRequestDto request)
        => await PostAsync<UserResponseDto>("/api/users", request);

    public async Task<ApiResponse<UserResponseDto>> UpdateAsync(Guid id, UpdateUserRequestDto request)
        => await PutAsync<UserResponseDto>($"/api/users/{id}", request);

    public async Task<ApiResponse> DeleteAsync(Guid id)
        => await base.DeleteAsync($"/api/users/{id}");

    public async Task<ApiResponse> ChangePasswordAsync(Guid id, ChangePasswordRequestDto request)
        => await PostAsync($"/api/users/{id}/change-password", request);

    public async Task<ApiResponse> ToggleStatusAsync(Guid id)
        => await PostAsync($"/api/users/{id}/toggle-status");
}
