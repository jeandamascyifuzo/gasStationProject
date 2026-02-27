using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;

namespace Escale.Web.Services.Implementations;

public class ApiCustomerService : BaseApiService, IApiCustomerService
{
    public ApiCustomerService(HttpClient httpClient) : base(httpClient) { }

    public async Task<ApiResponse<PagedResult<CustomerResponseDto>>> GetAllAsync(int page = 1, int pageSize = 20, string? searchTerm = null)
    {
        var query = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(searchTerm)) query += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
        return await GetAsync<PagedResult<CustomerResponseDto>>($"/api/customers{query}");
    }

    public async Task<ApiResponse<CustomerResponseDto>> GetByIdAsync(Guid id)
        => await GetAsync<CustomerResponseDto>($"/api/customers/{id}");

    public async Task<ApiResponse<CustomerResponseDto>> CreateAsync(CreateCustomerRequestDto request)
        => await PostAsync<CustomerResponseDto>("/api/customers", request);

    public async Task<ApiResponse<CustomerResponseDto>> UpdateAsync(Guid id, UpdateCustomerRequestDto request)
        => await PutAsync<CustomerResponseDto>($"/api/customers/{id}", request);

    public async Task<ApiResponse> DeleteAsync(Guid id)
        => await base.DeleteAsync($"/api/customers/{id}");
}
