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

    // Car CRUD
    public async Task<ApiResponse<CarDto>> AddCarAsync(Guid customerId, CarDto car)
        => await PostAsync<CarDto>($"/api/customers/{customerId}/cars", car);

    public async Task<ApiResponse<CarDto>> UpdateCarAsync(Guid customerId, Guid carId, CarDto car)
        => await PutAsync<CarDto>($"/api/customers/{customerId}/cars/{carId}", car);

    public async Task<ApiResponse> DeactivateCarAsync(Guid customerId, Guid carId)
        => await base.DeleteAsync($"/api/customers/{customerId}/cars/{carId}");

    // Subscriptions
    public async Task<ApiResponse<SubscriptionResponseDto>> TopUpSubscriptionAsync(TopUpSubscriptionRequestDto request)
        => await PostAsync<SubscriptionResponseDto>("/api/subscriptions/topup", request);

    public async Task<ApiResponse<SubscriptionResponseDto>> CancelSubscriptionAsync(Guid subscriptionId)
        => await PostAsync<SubscriptionResponseDto>($"/api/subscriptions/{subscriptionId}/cancel");

    public async Task<ApiResponse<List<SubscriptionResponseDto>>> GetSubscriptionHistoryAsync(Guid customerId)
        => await GetAsync<List<SubscriptionResponseDto>>($"/api/subscriptions/customer/{customerId}/history");
}
