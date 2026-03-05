using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Constants;
using Escale.API.Domain.Entities;
using Escale.API.Domain.Enums;
using Escale.API.DTOs.Sales;
using Escale.API.DTOs.Transactions;
using Escale.API.Hubs;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Escale.API.Services.Implementations;

public class SaleService : ISaleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IEBMService _ebmService;
    private readonly INotificationService _notificationService;
    private readonly IMemoryCache _cache;

    public SaleService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper,
        IEBMService ebmService, INotificationService notificationService, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _ebmService = ebmService;
        _notificationService = notificationService;
        _cache = cache;
    }

    public async Task<SaleResponseDto> CreateSaleAsync(CreateSaleRequestDto request)
    {
        var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var stepWatch = System.Diagnostics.Stopwatch.StartNew();

        var orgId = _currentUser.OrganizationId!.Value;
        var userId = _currentUser.UserId!.Value;

        // Begin explicit DB transaction — everything commits or rolls back together
        await using var dbTransaction = await _unitOfWork.Context.Database.BeginTransactionAsync();

        try
        {
            // Resolve fuel type (AsNoTracking — read-only lookup)
            FuelType? fuelType = null;
            if (request.FuelTypeId.HasValue)
            {
                fuelType = await _unitOfWork.FuelTypes.Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.Id == request.FuelTypeId.Value && f.OrganizationId == orgId);
            }
            if (fuelType == null && !string.IsNullOrEmpty(request.FuelType))
            {
                fuelType = await _unitOfWork.FuelTypes.Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.Name == request.FuelType && f.OrganizationId == orgId);
            }
            if (fuelType == null)
                throw new ArgumentException("Invalid fuel type");

            Console.WriteLine($"[Sale Timing] Fuel type lookup: {stepWatch.ElapsedMilliseconds}ms");
            stepWatch.Restart();

            // Calculate amounts (price per liter is tax-inclusive)
            var total = request.Liters * request.PricePerLiter;
            var vatAmount = Math.Round(total * BusinessRules.VATRate, 2);
            var subtotal = total - vatAmount;

            // Generate receipt number
            var receiptNumber = $"RCP{DateTime.UtcNow:yyyyMMddHHmmssfff}";

            // Get active shift (AsNoTracking — we only need the Id)
            var activeShift = await _unitOfWork.Shifts.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId && s.StationId == request.StationId && s.IsActive);

            // Parse payment method
            var paymentMethod = Enum.TryParse<PaymentMethod>(request.PaymentMethod.Replace(" ", ""), true, out var pm)
                ? pm : PaymentMethod.Cash;

            // Subscription deduction tracking
            Guid? subscriptionId = null;
            decimal? subscriptionDeduction = null;
            decimal? remainingBalanceAfter = null;

            // Walk-in customer info (Cash, Card, MobileMoney)
            Guid? customerId = null;
            string? customerName = request.Customer?.Name;
            string? customerPhone = request.Customer?.PhoneNumber;

            Console.WriteLine($"[Sale Timing] Shift lookup: {stepWatch.ElapsedMilliseconds}ms");
            stepWatch.Restart();

            if (paymentMethod == PaymentMethod.Credit)
            {
                // Credit = subscription customer: enforce all validation
                if (!request.SubscriptionId.HasValue)
                    throw new ArgumentException("SubscriptionId is required for Credit payment");

                if (request.Customer?.Id == null || request.Customer.Id == Guid.Empty)
                    throw new ArgumentException("Customer is required for Credit payment");

                // Verify customer exists and is active
                var customer = await _unitOfWork.Customers.Query()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == request.Customer.Id.Value && c.OrganizationId == orgId);

                if (customer == null)
                    throw new InvalidOperationException("Customer not found");

                if (!customer.IsActive)
                    throw new InvalidOperationException("Customer account is not active");

                customerId = customer.Id;
                customerName = customer.Name;
                customerPhone = customer.PhoneNumber;

                // Validate subscription (tracked — we need to update balance)
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
                    await dbTransaction.CommitAsync();
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

            Console.WriteLine($"[Sale Timing] Subscription/customer validation: {stepWatch.ElapsedMilliseconds}ms");
            stepWatch.Restart();

            // Check org-level EBM settings — cached to avoid DB hit per sale
            bool ebmSent = false;
            string? ebmReceiptUrl = null;

            var orgSettings = await GetCachedOrgSettingsAsync(orgId);

            if (orgSettings?.EBMEnabled == true)
            {
                if (string.IsNullOrEmpty(fuelType.EBMVariantId))
                {
                    throw new InvalidOperationException(
                        $"EBM is required but fuel type '{fuelType.Name}' has no EBM variant configured. Contact admin.");
                }

                var ebmResult = await _ebmService.SendSaleReceiptAsync(
                    orgId, fuelType.EBMVariantId, request.Liters, customerName, customerPhone);

                if (!ebmResult.Success)
                {
                    var friendlyMessage = ParseEbmError(ebmResult.ErrorMessage, fuelType.Name);
                    throw new InvalidOperationException(friendlyMessage);
                }

                ebmSent = true;
                ebmReceiptUrl = ebmResult.ReceiptCode;
            }

            Console.WriteLine($"[Sale Timing] EBM receipt: {stepWatch.ElapsedMilliseconds}ms");
            stepWatch.Restart();

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
                EBMSent = ebmSent,
                EBMCode = ebmReceiptUrl,
                EBMSentAt = ebmSent ? DateTime.UtcNow : null,
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

            // Save and commit — transaction, inventory, subscription all in one atomic commit
            await _unitOfWork.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            Console.WriteLine($"[Sale Timing] DB save + commit: {stepWatch.ElapsedMilliseconds}ms");
            stepWatch.Restart();

            // Fire-and-forget — don't block the sale response for SignalR
            _ = _notificationService.NotifyDataChangedAsync(orgId, NotificationConstants.SaleCompleted);

            totalStopwatch.Stop();
            Console.WriteLine($"[Sale Timing] TOTAL: {totalStopwatch.ElapsedMilliseconds}ms");

            return new SaleResponseDto
            {
                Success = true,
                Message = "Sale completed successfully",
                Sale = new CompletedSaleDto
                {
                    Id = transaction.Id,
                    ReceiptNumber = receiptNumber,
                    EBMReceiptUrl = ebmReceiptUrl,
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
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<TransactionResponseDto>> GetRecentSalesAsync(Guid? stationId, int count = 10)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var query = _unitOfWork.Transactions.Query()
            .AsNoTracking()
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

    private async Task<OrganizationSettings?> GetCachedOrgSettingsAsync(Guid orgId)
    {
        var cacheKey = $"org_settings_{orgId}";
        if (_cache.TryGetValue(cacheKey, out OrganizationSettings? cached))
            return cached;

        var settings = await _unitOfWork.OrganizationSettings.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.OrganizationId == orgId);

        if (settings != null)
        {
            _cache.Set(cacheKey, settings, TimeSpan.FromMinutes(5));
        }

        return settings;
    }

    private static string ParseEbmError(string? ebmError, string fuelTypeName)
    {
        if (string.IsNullOrEmpty(ebmError))
            return "Sale failed due to an EBM error. Please try again or contact support.";

        var lower = ebmError.ToLower();

        if (lower.Contains("insufficient stock"))
            return $"Insufficient stock for {fuelTypeName}. Please refill stock before making this sale.";

        if (lower.Contains("not found"))
            return $"{fuelTypeName} is not registered in EBM. Please contact your administrator.";

        if (lower.Contains("unauthorized") || lower.Contains("401"))
            return "EBM authentication failed. Please contact your administrator to check EBM settings.";

        if (lower.Contains("timeout") || lower.Contains("timed out"))
            return "EBM server is not responding. Please try again in a few moments.";

        if (lower.Contains("connection") || lower.Contains("network"))
            return "Cannot connect to EBM server. Please check your internet connection and try again.";

        if (lower.Contains("500") || lower.Contains("internalservererror") || lower.Contains("internal server error"))
            return "EBM server encountered an internal error. Please try again or contact support.";

        if (lower.Contains("503") || lower.Contains("service unavailable"))
            return "EBM server is temporarily unavailable. Please try again in a few moments.";

        if (lower.Contains("429") || lower.Contains("rate limit") || lower.Contains("too many"))
            return "Too many requests to EBM server. Please wait a moment and try again.";

        return "Sale could not be completed due to an EBM error. Please try again or contact support.";
    }
}
