using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Entities;
using Escale.API.Domain.Enums;
using Escale.API.DTOs.Subscriptions;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class SubscriptionService : ISubscriptionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public SubscriptionService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<SubscriptionResponseDto> TopUpAsync(TopUpSubscriptionRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;

        var customer = await _unitOfWork.Customers.Query()
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId && c.OrganizationId == orgId && c.IsActive)
            ?? throw new KeyNotFoundException("Customer not found or inactive");

        // Find current active subscription
        var currentSub = await _unitOfWork.Subscriptions.Query()
            .FirstOrDefaultAsync(s => s.CustomerId == request.CustomerId
                                   && s.OrganizationId == orgId
                                   && s.Status == SubscriptionStatus.Active);

        decimal previousBalance = 0;

        if (currentSub != null)
        {
            previousBalance = currentSub.RemainingBalance;
            currentSub.Status = SubscriptionStatus.Inactive;
            currentSub.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Subscriptions.Update(currentSub);
        }

        var newSubscription = new Subscription
        {
            OrganizationId = orgId,
            CustomerId = request.CustomerId,
            PreviousBalance = previousBalance,
            TopUpAmount = request.TopUpAmount,
            TotalAmount = previousBalance + request.TopUpAmount,
            RemainingBalance = previousBalance + request.TopUpAmount,
            StartDate = DateTime.UtcNow,
            ExpiryDate = request.ExpiryDate,
            Status = SubscriptionStatus.Active
        };

        await _unitOfWork.Subscriptions.AddAsync(newSubscription);
        await _unitOfWork.SaveChangesAsync();

        // Reload with customer for mapping
        var saved = await _unitOfWork.Subscriptions.Query()
            .Include(s => s.Customer)
            .FirstAsync(s => s.Id == newSubscription.Id);

        return _mapper.Map<SubscriptionResponseDto>(saved);
    }

    public async Task<SubscriptionResponseDto?> GetActiveSubscriptionAsync(Guid customerId)
    {
        var orgId = _currentUser.OrganizationId!.Value;

        var sub = await _unitOfWork.Subscriptions.Query()
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.CustomerId == customerId
                                   && s.OrganizationId == orgId
                                   && s.Status == SubscriptionStatus.Active);

        if (sub == null) return null;

        // Auto-expire if past expiry date
        if (sub.ExpiryDate.HasValue && sub.ExpiryDate.Value < DateTime.UtcNow)
        {
            sub.Status = SubscriptionStatus.Expired;
            sub.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Subscriptions.Update(sub);
            await _unitOfWork.SaveChangesAsync();
            return null;
        }

        return _mapper.Map<SubscriptionResponseDto>(sub);
    }

    public async Task<List<SubscriptionResponseDto>> GetSubscriptionHistoryAsync(Guid customerId)
    {
        var orgId = _currentUser.OrganizationId!.Value;

        var subs = await _unitOfWork.Subscriptions.Query()
            .Include(s => s.Customer)
            .Where(s => s.CustomerId == customerId && s.OrganizationId == orgId)
            .OrderByDescending(s => s.StartDate)
            .ToListAsync();

        return _mapper.Map<List<SubscriptionResponseDto>>(subs);
    }

    public async Task<SubscriptionCustomerLookupResponseDto> LookupByCarAsync(LookupCarRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;

        // Find car by plate number
        var car = await _unitOfWork.Cars.Query()
            .Include(c => c.Customer)
                .ThenInclude(c => c.Subscriptions)
            .FirstOrDefaultAsync(c => c.PlateNumber.ToLower() == request.PlateNumber.ToLower()
                                   && c.Customer.OrganizationId == orgId
                                   && !c.IsDeleted);

        if (car == null)
        {
            return new SubscriptionCustomerLookupResponseDto
            {
                ValidationError = "Car not found with this plate number"
            };
        }

        // Verify PIN
        if (!BCrypt.Net.BCrypt.Verify(request.PIN, car.PINHash))
        {
            return new SubscriptionCustomerLookupResponseDto
            {
                ValidationError = "Invalid PIN"
            };
        }

        if (!car.IsActive)
        {
            return new SubscriptionCustomerLookupResponseDto
            {
                ValidationError = "This car is not active"
            };
        }

        var customer = car.Customer;

        if (!customer.IsActive)
        {
            return new SubscriptionCustomerLookupResponseDto
            {
                ValidationError = "Customer account is not active"
            };
        }

        // Get active subscription
        var activeSub = customer.Subscriptions
            .FirstOrDefault(s => s.Status == SubscriptionStatus.Active && !s.IsDeleted);

        // Auto-expire if past expiry
        if (activeSub != null && activeSub.ExpiryDate.HasValue && activeSub.ExpiryDate.Value < DateTime.UtcNow)
        {
            activeSub.Status = SubscriptionStatus.Expired;
            activeSub.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Subscriptions.Update(activeSub);
            await _unitOfWork.SaveChangesAsync();
            activeSub = null;
        }

        if (activeSub == null)
        {
            return new SubscriptionCustomerLookupResponseDto
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                CustomerIsActive = true,
                CarId = car.Id,
                PlateNumber = car.PlateNumber,
                CarMake = car.Make,
                CarModel = car.Model,
                CarIsActive = true,
                ValidationError = "No active subscription found for this customer"
            };
        }

        var hasSufficientBalance = true;
        if (request.SaleAmount.HasValue && request.SaleAmount.Value > 0)
        {
            hasSufficientBalance = activeSub.RemainingBalance >= request.SaleAmount.Value;
        }

        return new SubscriptionCustomerLookupResponseDto
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            PhoneNumber = customer.PhoneNumber,
            CustomerIsActive = true,
            CarId = car.Id,
            PlateNumber = car.PlateNumber,
            CarMake = car.Make,
            CarModel = car.Model,
            CarIsActive = true,
            ActiveSubscriptionId = activeSub.Id,
            RemainingBalance = activeSub.RemainingBalance,
            SubscriptionExpiryDate = activeSub.ExpiryDate,
            HasSufficientBalance = hasSufficientBalance
        };
    }

    public async Task<SubscriptionResponseDto> CancelSubscriptionAsync(Guid subscriptionId)
    {
        var orgId = _currentUser.OrganizationId!.Value;

        var sub = await _unitOfWork.Subscriptions.Query()
            .Include(s => s.Customer)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Subscription not found");

        sub.Status = SubscriptionStatus.Cancelled;
        sub.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Subscriptions.Update(sub);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SubscriptionResponseDto>(sub);
    }
}
