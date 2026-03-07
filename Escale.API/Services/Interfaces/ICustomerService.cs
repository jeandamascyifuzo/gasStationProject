using Escale.API.DTOs.Common;
using Escale.API.DTOs.Customers;

namespace Escale.API.Services.Interfaces;

public interface ICustomerService
{
    Task<PagedResult<CustomerResponseDto>> GetCustomersAsync(PagedRequest request, string? type = null);
    Task<CustomerResponseDto> GetCustomerByIdAsync(Guid id);
    Task<List<CustomerResponseDto>> SearchCustomersAsync(string term);
    Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerRequestDto request);
    Task<CustomerResponseDto> UpdateCustomerAsync(Guid id, UpdateCustomerRequestDto request);
    Task DeleteCustomerAsync(Guid id);
    Task<CarResponseDto> AddCarAsync(Guid customerId, CarDto request);
    Task<CarResponseDto> UpdateCarAsync(Guid customerId, Guid carId, CarDto request);
    Task DeactivateCarAsync(Guid customerId, Guid carId);
    Task ReactivateCarAsync(Guid customerId, Guid carId);
    Task<CustomerTransactionsPagedResult> GetCustomerTransactionsAsync(Guid customerId, int page = 1, int pageSize = 20,
        DateTime? startDate = null, DateTime? endDate = null, Guid? stationId = null, string? search = null);
}
