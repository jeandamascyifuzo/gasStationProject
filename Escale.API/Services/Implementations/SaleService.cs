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

        // Calculate amounts
        var subtotal = request.Liters * request.PricePerLiter;
        var vatAmount = Math.Round(subtotal * BusinessRules.VATRate, 2);
        var total = subtotal + vatAmount;

        // Generate receipt number
        var receiptNumber = $"RCP{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        // Generate EBM code
        var ebmCode = $"EBM{Random.Shared.Next(100000, 999999)}";

        // Get active shift
        var activeShift = await _unitOfWork.Shifts.Query()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.StationId == request.StationId && s.IsActive);

        // Resolve customer
        Guid? customerId = request.Customer?.Id;
        string? customerName = request.Customer?.Name;
        string? customerPhone = request.Customer?.PhoneNumber;

        // Parse payment method
        var paymentMethod = Enum.TryParse<PaymentMethod>(request.PaymentMethod.Replace(" ", ""), true, out var pm) ? pm : PaymentMethod.Cash;

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
            EBMCode = ebmCode
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

        // Handle credit
        if (paymentMethod == PaymentMethod.Credit && customerId.HasValue)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId.Value);
            if (customer != null)
            {
                customer.CurrentCredit += total;
                _unitOfWork.Customers.Update(customer);
            }
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
                PaymentMethod = paymentMethod.ToString()
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
