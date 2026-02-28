using Escale.API.DTOs.Common;
using Escale.API.DTOs.Customers;

namespace Escale.API.Services.Interfaces;

public interface ICustomerService
{
    Task<PagedResult<CustomerResponseDto>> GetCustomersAsync(PagedRequest request);
    Task<CustomerResponseDto> GetCustomerByIdAsync(Guid id);
    Task<List<CustomerResponseDto>> SearchCustomersAsync(string term);
    Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerRequestDto request);
    Task<CustomerResponseDto> UpdateCustomerAsync(Guid id, UpdateCustomerRequestDto request);
    Task DeleteCustomerAsync(Guid id);
    Task<CarResponseDto> AddCarAsync(Guid customerId, CarDto request);
    Task<CarResponseDto> UpdateCarAsync(Guid customerId, Guid carId, CarDto request);
    Task DeactivateCarAsync(Guid customerId, Guid carId);
}
