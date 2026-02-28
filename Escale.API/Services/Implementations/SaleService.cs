using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Constants;
using Escale.API.Domain.Entities;
using Escale.API.Domain.Enums;
using Escale.API.DTOs.Sales;
using Escale.API.DTOs.Transactions;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class SaleService : ISaleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public SaleService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<SaleResponseDto> CreateSaleAsync(CreateSaleRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var userId = _currentUser.UserId!.Value;

        // Resolve fuel type
        FuelType? fuelType = null;
        if (request.FuelTypeId.HasValue)
        {
            fuelType = await _unitOfWork.FuelTypes.Query()
                .FirstOrDefaultAsync(f => f.Id == request.FuelTypeId.Value && f.OrganizationId == orgId);
        }
        if (fuelType == null && !string.IsNullOrEmpty(request.FuelType))
        {
            fuelType = await _unitOfWork.FuelTypes.Query()
                .FirstOrDefaultAsync(f => f.Name == request.FuelType && f.OrganizationId == orgId);
        }
        if (fuelType == null)
            throw new ArgumentException("Invalid fuel type");

        // Calculate amounts (price per liter is tax-inclusive)
        var total = request.Liters * request.PricePerLiter;
        var vatAmount = Math.Round(total * BusinessRules.VATRate, 2);
        var subtotal = total - vatAmount;

        // Generate receipt number and EBM code
        var receiptNumber = $"RCP{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        var ebmCode = $"EBM{Random.Shared.Next(100000, 999999)}";

        // Get active shift
        var activeShift = await _unitOfWork.Shifts.Query()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.StationId == request.StationId && s.IsActive);

        // Parse payment method
        var paymentMethod = Enum.TryParse<PaymentMethod>(request.PaymentMethod.Replace(" ", ""), true, out var pm)
            ? pm : PaymentMethod.Cash;

        // Subscription deduction tracking
        Guid? subscriptionId = null;
        decimal? subscriptionDeduction = null;
        decimal? remainingBalanceAfter = null;

        // Walk-in customer info (Cash, Card, MobileMoney)
        // Just record name/phone on the transaction — no customer entity linking
        Guid? customerId = null;
        string? customerName = request.Customer?.Name;
        string? customerPhone = request.Customer?.PhoneNumber;

        if (paymentMethod == PaymentMethod.Credit)
        {
            // Credit = subscription customer: enforce all validation
            if (!request.SubscriptionId.HasValue)
                throw new ArgumentException("SubscriptionId is required for Credit payment");

            if (request.Customer?.Id == null || request.Customer.Id == Guid.Empty)
                throw new ArgumentException("Customer is required for Credit payment");

            // Verify customer exists and is active
            var customer = await _unitOfWork.Customers.Query()
                .FirstOrDefaultAsync(c => c.Id == request.Customer.Id.Value && c.OrganizationId == orgId);

            if (customer == null)
                throw new InvalidOperationException("Customer not found");

            if (!customer.IsActive)
                throw new InvalidOperationException("Customer account is not active");

            customerId = customer.Id;
            customerName = customer.Name;
            customerPhone = customer.PhoneNumber;

            // Validate subscription
            var subscription = await _unitOfWork.Subscriptions.Query()
                .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId.Value
                                       && s.OrganizationId == orgId
                                       && s.Status == SubscriptionStatus.Active);

            if (subscription == null)
                throw new InvalidOperationException("No active subscription found");

            if (subscription.CustomerId != customer.Id)
                throw new InvalidOperationException("Subscription does not belong to this customer");

            // Check expiry
            if (subscription.ExpiryDate.HasValue && subscription.ExpiryDate.Value < DateTime.UtcNow)
            {
                subscription.Status = SubscriptionStatus.Expired;
                _unitOfWork.Subscriptions.Update(subscription);
                await _unitOfWork.SaveChangesAsync();
                throw new InvalidOperationException("Subscription has expired");
            }

            // Check balance
            if (subscription.RemainingBalance < total)
                throw new InvalidOperationException(
                    $"Insufficient subscription balance. Available: {subscription.RemainingBalance:N0} RWF, Required: {total:N0} RWF");

            // Deduct from subscription balance
            subscription.RemainingBalance -= total;
            subscription.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Subscriptions.Update(subscription);

            subscriptionId = subscription.Id;
            subscriptionDeduction = total;
            remainingBalanceAfter = subscription.RemainingBalance;
        }

        var transaction = new Transaction
        {
            OrganizationId = orgId,
            ReceiptNumber = receiptNumber,
            TransactionDate = DateTime.UtcNow,
            StationId = request.StationId,
            FuelTypeId = fuelType.Id,
            Liters = request.Liters,
            PricePerLiter = request.PricePerLiter,
            Subtotal = subtotal,
            VATAmount = vatAmount,
            Total = total,
            PaymentMethod = paymentMethod,
            Status = TransactionStatus.Completed,
            CustomerId = customerId,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            CashierId = userId,
            ShiftId = activeShift?.Id,
            EBMSent = true,
            EBMCode = ebmCode,
            SubscriptionId = subscriptionId,
            SubscriptionDeduction = subscriptionDeduction
        };

        await _unitOfWork.Transactions.AddAsync(transaction);

        // Decrement inventory
        var inventoryItem = await _unitOfWork.InventoryItems.Query()
            .FirstOrDefaultAsync(i => i.StationId == request.StationId && i.FuelTypeId == fuelType.Id && i.OrganizationId == orgId);

        if (inventoryItem != null)
        {
            inventoryItem.CurrentLevel = Math.Max(0, inventoryItem.CurrentLevel - request.Liters);
            _unitOfWork.InventoryItems.Update(inventoryItem);
        }

        await _unitOfWork.SaveChangesAsync();

        return new SaleResponseDto
        {
            Success = true,
            Message = "Sale completed successfully",
            Sale = new CompletedSaleDto
            {
                Id = transaction.Id,
                ReceiptNumber = receiptNumber,
                EBMCode = ebmCode,
                TransactionDate = transaction.TransactionDate,
                FuelType = fuelType.Name,
                Liters = request.Liters,
                PricePerLiter = request.PricePerLiter,
                Subtotal = subtotal,
                VAT = vatAmount,
                Total = total,
                PaymentMethod = paymentMethod.ToString(),
                SubscriptionDeduction = subscriptionDeduction,
                SubscriptionRemainingBalance = remainingBalanceAfter
            }
        };
    }

    public async Task<List<TransactionResponseDto>> GetRecentSalesAsync(Guid? stationId, int count = 10)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var query = _unitOfWork.Transactions.Query()
            .Include(t => t.FuelType)
            .Include(t => t.Cashier)
            .Include(t => t.Station)
            .Where(t => t.OrganizationId == orgId);

        if (stationId.HasValue)
            query = query.Where(t => t.StationId == stationId.Value);

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .Take(count)
            .ToListAsync();

        return _mapper.Map<List<TransactionResponseDto>>(transactions);
    }
}
