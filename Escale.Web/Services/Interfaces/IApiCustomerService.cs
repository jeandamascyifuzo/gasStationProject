using Escale.Web.Models.Api;

namespace Escale.Web.Services.Interfaces;

public interface IApiCustomerService
{
    Task<ApiResponse<PagedResult<CustomerResponseDto>>> GetAllAsync(int page = 1, int pageSize = 20, string? searchTerm = null);
    Task<ApiResponse<CustomerResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<CustomerResponseDto>> CreateAsync(CreateCustomerRequestDto request);
    Task<ApiResponse<CustomerResponseDto>> UpdateAsync(Guid id, UpdateCustomerRequestDto request);
    Task<ApiResponse> DeleteAsync(Guid id);
}
