using Escale.Web.Models.Api;

namespace Escale.Web.Services.Interfaces;

public interface IApiCustomerService
{
    Task<ApiResponse<PagedResult<CustomerResponseDto>>> GetAllAsync(int page = 1, int pageSize = 20, string? searchTerm = null);
    Task<ApiResponse<CustomerResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<CustomerResponseDto>> CreateAsync(CreateCustomerRequestDto request);
    Task<ApiResponse<CustomerResponseDto>> UpdateAsync(Guid id, UpdateCustomerRequestDto request);
    Task<ApiResponse> DeleteAsync(Guid id);

    // Car CRUD
    Task<ApiResponse<CarDto>> AddCarAsync(Guid customerId, CarDto car);
    Task<ApiResponse<CarDto>> UpdateCarAsync(Guid customerId, Guid carId, CarDto car);
    Task<ApiResponse> DeactivateCarAsync(Guid customerId, Guid carId);

    // Subscriptions
    Task<ApiResponse<SubscriptionResponseDto>> TopUpSubscriptionAsync(TopUpSubscriptionRequestDto request);
    Task<ApiResponse<SubscriptionResponseDto>> CancelSubscriptionAsync(Guid subscriptionId);
    Task<ApiResponse<List<SubscriptionResponseDto>>> GetSubscriptionHistoryAsync(Guid customerId);
}
