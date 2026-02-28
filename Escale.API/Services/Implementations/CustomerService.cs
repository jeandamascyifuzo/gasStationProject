using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Entities;
using Escale.API.Domain.Enums;
using Escale.API.DTOs.Common;
using Escale.API.DTOs.Customers;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public CustomerService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<PagedResult<CustomerResponseDto>> GetCustomersAsync(PagedRequest request)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var query = _unitOfWork.Customers.Query()
            .Include(c => c.Cars)
            .Include(c => c.Subscriptions)
            .Where(c => c.OrganizationId == orgId);

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(term) ||
                                     (c.PhoneNumber != null && c.PhoneNumber.Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var customers = await query
            .OrderBy(c => c.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<CustomerResponseDto>
        {
            Items = _mapper.Map<List<CustomerResponseDto>>(customers),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<CustomerResponseDto> GetCustomerByIdAsync(Guid id)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var customer = await _unitOfWork.Customers.Query()
            .Include(c => c.Cars)
            .Include(c => c.Subscriptions)
            .FirstOrDefaultAsync(c => c.Id == id && c.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Customer not found");
        return _mapper.Map<CustomerResponseDto>(customer);
    }

    public async Task<List<CustomerResponseDto>> SearchCustomersAsync(string term)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var lower = term.ToLower();
        var customers = await _unitOfWork.Customers.Query()
            .Include(c => c.Cars)
            .Include(c => c.Subscriptions)
            .Where(c => c.OrganizationId == orgId &&
                        (c.Name.ToLower().Contains(lower) ||
                         (c.PhoneNumber != null && c.PhoneNumber.Contains(term)) ||
                         c.Cars.Any(car => car.PlateNumber.ToLower().Contains(lower))))
            .Take(20)
            .ToListAsync();
        return _mapper.Map<List<CustomerResponseDto>>(customers);
    }

    public async Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;

        var customer = new Customer
        {
            OrganizationId = orgId,
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Type = Enum.TryParse<CustomerType>(request.Type, out var ct) ? ct : CustomerType.Individual,
            TIN = request.TIN,
            CreditLimit = request.CreditLimit,
            IsActive = true
        };

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        if (request.Cars?.Any() == true)
        {
            foreach (var carDto in request.Cars)
            {
                if (string.IsNullOrWhiteSpace(carDto.PIN))
                    throw new ArgumentException($"PIN is required for car {carDto.PlateNumber}");

                await _unitOfWork.Cars.AddAsync(new Car
                {
                    CustomerId = customer.Id,
                    PlateNumber = carDto.PlateNumber,
                    Make = carDto.Make,
                    Model = carDto.Model,
                    Year = carDto.Year,
                    PINHash = BCrypt.Net.BCrypt.HashPassword(carDto.PIN),
                    IsActive = carDto.IsActive
                });
            }
            await _unitOfWork.SaveChangesAsync();
        }

        return await GetCustomerByIdAsync(customer.Id);
    }

    public async Task<CustomerResponseDto> UpdateCustomerAsync(Guid id, UpdateCustomerRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var customer = await _unitOfWork.Customers.Query()
            .FirstOrDefaultAsync(c => c.Id == id && c.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Customer not found");

        customer.Name = request.Name;
        customer.PhoneNumber = request.PhoneNumber;
        customer.Email = request.Email;
        customer.Type = Enum.TryParse<CustomerType>(request.Type, out var ct) ? ct : CustomerType.Individual;
        customer.TIN = request.TIN;
        customer.CreditLimit = request.CreditLimit;
        customer.IsActive = request.IsActive;

        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();
        return await GetCustomerByIdAsync(customer.Id);
    }

    public async Task DeleteCustomerAsync(Guid id)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var customer = await _unitOfWork.Customers.Query()
            .FirstOrDefaultAsync(c => c.Id == id && c.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Customer not found");
        _unitOfWork.Customers.Remove(customer);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<CarResponseDto> AddCarAsync(Guid customerId, CarDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var customer = await _unitOfWork.Customers.Query()
            .FirstOrDefaultAsync(c => c.Id == customerId && c.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Customer not found");

        if (string.IsNullOrWhiteSpace(request.PIN))
            throw new ArgumentException("PIN is required");

        var car = new Car
        {
            CustomerId = customerId,
            PlateNumber = request.PlateNumber,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            PINHash = BCrypt.Net.BCrypt.HashPassword(request.PIN),
            IsActive = request.IsActive
        };

        await _unitOfWork.Cars.AddAsync(car);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CarResponseDto>(car);
    }

    public async Task<CarResponseDto> UpdateCarAsync(Guid customerId, Guid carId, CarDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;

        var car = await _unitOfWork.Cars.Query()
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == carId && c.CustomerId == customerId && c.Customer.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Car not found");

        car.PlateNumber = request.PlateNumber;
        car.Make = request.Make;
        car.Model = request.Model;
        car.Year = request.Year;
        car.IsActive = request.IsActive;

        if (!string.IsNullOrWhiteSpace(request.PIN))
        {
            car.PINHash = BCrypt.Net.BCrypt.HashPassword(request.PIN);
        }

        _unitOfWork.Cars.Update(car);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CarResponseDto>(car);
    }

    public async Task DeactivateCarAsync(Guid customerId, Guid carId)
    {
        var orgId = _currentUser.OrganizationId!.Value;

        var car = await _unitOfWork.Cars.Query()
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == carId && c.CustomerId == customerId && c.Customer.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Car not found");

        car.IsActive = false;
        _unitOfWork.Cars.Update(car);
        await _unitOfWork.SaveChangesAsync();
    }
}
