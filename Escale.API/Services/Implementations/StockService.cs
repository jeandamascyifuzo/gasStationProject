using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Entities;
using Escale.API.DTOs.Stock;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class StockService : IStockService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IEBMService _ebmService;
    private readonly ILogger<StockService> _logger;

    public StockService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper,
        IEBMService ebmService, ILogger<StockService> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _ebmService = ebmService;
        _logger = logger;
    }

    public async Task<List<StockLevelDto>> GetStockLevelsAsync(Guid? stationId = null)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var query = _unitOfWork.InventoryItems.Query()
            .Include(i => i.FuelType)
            .Where(i => i.OrganizationId == orgId);

        if (stationId.HasValue)
            query = query.Where(i => i.StationId == stationId.Value);

        var items = await query.ToListAsync();
        var dtos = _mapper.Map<List<StockLevelDto>>(items);

        foreach (var dto in dtos)
        {
            var pct = dto.Capacity > 0 ? dto.CurrentLevel / dto.Capacity : 0;
            dto.Status = pct < 0.10m ? "Critical" : pct < 0.25m ? "Low Stock" : "Normal";
        }

        return dtos;
    }

    public async Task RecordRefillAsync(Guid stationId, string fuelType, decimal quantity, decimal unitCost, string? supplierName, string? invoiceNumber, DateTime refillDate)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var userId = _currentUser.UserId!.Value;

        var item = await _unitOfWork.InventoryItems.Query()
            .Include(i => i.FuelType)
            .FirstOrDefaultAsync(i => i.StationId == stationId && i.FuelType.Name == fuelType && i.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Inventory item not found for this station and fuel type");

        var newLevel = Math.Min(item.Capacity, item.CurrentLevel + quantity);
        bool ebmStockUpdated = false;

        // Check if EBM is enabled for this org
        var orgSettings = await _unitOfWork.OrganizationSettings.Query()
            .FirstOrDefaultAsync(s => s.OrganizationId == orgId);
        var ebmEnabled = orgSettings?.EBMEnabled == true;

        // If EBM is enabled, stock update is REQUIRED — no partial save
        if (ebmEnabled)
        {
            if (string.IsNullOrEmpty(item.EBMStockId))
                throw new InvalidOperationException(
                    "EBM is enabled but this inventory item has no EBM Stock ID. " +
                    "Please delete and re-create the fuel type to register it with EBM.");

            _logger.LogInformation("Updating EBM stock for item {ItemId}: Quantity={Quantity}, StockId={StockId}",
                item.Id, quantity, item.EBMStockId);

            var ebmSuccess = await _ebmService.UpdateStockAsync(orgId, item.EBMStockId, quantity);

            if (!ebmSuccess)
                throw new InvalidOperationException(
                    "Failed to update stock in EBM. Refill was NOT recorded. Please try again.");

            _logger.LogInformation("EBM stock update succeeded for item {ItemId}", item.Id);
            ebmStockUpdated = true;
        }

        // EBM succeeded (or not needed) — now save to DB
        try
        {
            var refill = new RefillRecord
            {
                InventoryItemId = item.Id,
                Quantity = quantity,
                UnitCost = unitCost,
                TotalCost = quantity * unitCost,
                SupplierName = supplierName,
                InvoiceNumber = invoiceNumber,
                RefillDate = refillDate,
                RecordedById = userId
            };

            await _unitOfWork.RefillRecords.AddAsync(refill);
            item.CurrentLevel = newLevel;
            item.LastRefillDate = refillDate;
            _unitOfWork.InventoryItems.Update(item);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // DB failed — EBM stock was already added and cannot be subtracted via API
            if (ebmStockUpdated)
            {
                _logger.LogError("CRITICAL: DB save failed but EBM stock already increased by {Quantity} for item {ItemId} (StockId={StockId}). Manual fix required.",
                    quantity, item.Id, item.EBMStockId);
            }
            throw new InvalidOperationException(
                $"Failed to save refill to database. EBM stock was already updated — please contact support. Error: {ex.Message}");
        }
    }
}
