using Escale.API.DTOs.Common;
using Escale.API.DTOs.Customers;
using Escale.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<CustomerResponseDto>>>> GetCustomers([FromQuery] PagedRequest request)
    {
        var result = await _customerService.GetCustomersAsync(request);
        return Ok(ApiResponse<PagedResult<CustomerResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<CustomerResponseDto>>>> SearchCustomers([FromQuery] string term)
    {
        var result = await _customerService.SearchCustomersAsync(term ?? "");
        return Ok(ApiResponse<List<CustomerResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> GetCustomer(Guid id)
    {
        var result = await _customerService.GetCustomerByIdAsync(id);
        return Ok(ApiResponse<CustomerResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> CreateCustomer([FromBody] CreateCustomerRequestDto request)
    {
        var result = await _customerService.CreateCustomerAsync(request);
        return Ok(ApiResponse<CustomerResponseDto>.SuccessResponse(result, "Customer created"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> UpdateCustomer(Guid id, [FromBody] UpdateCustomerRequestDto request)
    {
        var result = await _customerService.UpdateCustomerAsync(id, request);
        return Ok(ApiResponse<CustomerResponseDto>.SuccessResponse(result, "Customer updated"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteCustomer(Guid id)
    {
        await _customerService.DeleteCustomerAsync(id);
        return Ok(ApiResponse.SuccessResponse("Customer deleted"));
    }

    [HttpPost("{customerId}/cars")]
    public async Task<ActionResult<ApiResponse<CarResponseDto>>> AddCar(Guid customerId, [FromBody] CarDto request)
    {
        var result = await _customerService.AddCarAsync(customerId, request);
        return Ok(ApiResponse<CarResponseDto>.SuccessResponse(result, "Car added"));
    }

    [HttpPut("{customerId}/cars/{carId}")]
    public async Task<ActionResult<ApiResponse<CarResponseDto>>> UpdateCar(Guid customerId, Guid carId, [FromBody] CarDto request)
    {
        var result = await _customerService.UpdateCarAsync(customerId, carId, request);
        return Ok(ApiResponse<CarResponseDto>.SuccessResponse(result, "Car updated"));
    }

    [HttpDelete("{customerId}/cars/{carId}")]
    public async Task<ActionResult<ApiResponse>> DeactivateCar(Guid customerId, Guid carId)
    {
        await _customerService.DeactivateCarAsync(customerId, carId);
        return Ok(ApiResponse.SuccessResponse("Car deactivated"));
    }
}
