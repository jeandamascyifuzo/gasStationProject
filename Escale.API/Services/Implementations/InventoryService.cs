using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Entities;
using Escale.API.DTOs.Inventory;
using Escale.API.Hubs;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IEBMService _ebmService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<InventoryService> _logger;
    private readonly IAuditLogger _audit;

    public InventoryService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper,
        IEBMService ebmService, INotificationService notificationService, ILogger<InventoryService> logger, IAuditLogger audit)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _ebmService = ebmService;
        _notificationService = notificationService;
        _logger = logger;
        _audit = audit;
    }

    public async Task<List<InventoryItemResponseDto>> GetInventoryAsync(Guid? stationId = null)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var query = _unitOfWork.InventoryItems.Query()
            .AsNoTracking()
            .Include(i => i.Station)
            .Include(i => i.FuelType)
            .Where(i => i.OrganizationId == orgId);

        if (stationId.HasValue)
            query = query.Where(i => i.StationId == stationId.Value);

        var items = await query.OrderBy(i => i.Station.Name).ThenBy(i => i.FuelType.Name).ToListAsync();

        // Fetch EBM stock in parallel — one call per unique variantId
        var ebmStockByVariant = new Dictionary<string, decimal?>();
        var orgSettings = await _unitOfWork.OrganizationSettings.Query()
            .AsNoTracking().FirstOrDefaultAsync(s => s.OrganizationId == orgId);

        if (orgSettings?.EBMEnabled == true)
        {
            var uniqueVariants = items
                .Where(i => !string.IsNullOrEmpty(i.FuelType.EBMVariantId))
                .Select(i => i.FuelType.EBMVariantId!)
                .Distinct()
                .ToList();

            var ebmTasks = uniqueVariants.Select(async variantId =>
            {
                var result = await _ebmService.CheckStockAsync(orgId, variantId);
                return (variantId, result.Success ? result.Stock : null);
            });

            foreach (var (variantId, stock) in await Task.WhenAll(ebmTasks))
                ebmStockByVariant[variantId] = stock;
        }

        var dtos = _mapper.Map<List<InventoryItemResponseDto>>(items);

        for (int i = 0; i < dtos.Count; i++)
        {
            var pct = dtos[i].PercentageFull / 100;
            dtos[i].Status = pct < 0.10m ? "Critical" : pct < 0.25m ? "Low Stock" : "Normal";

            var variantId = items[i].FuelType.EBMVariantId;
            if (!string.IsNullOrEmpty(variantId) && ebmStockByVariant.TryGetValue(variantId, out var stock))
                dtos[i].EBMStock = stock;
        }

        return dtos;
    }

    public async Task<List<RefillRecordResponseDto>> GetRefillsAsync(int count = 20)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var refills = await _unitOfWork.RefillRecords.Query()
            .AsNoTracking()
            .Include(r => r.InventoryItem).ThenInclude(i => i.Station)
            .Include(r => r.InventoryItem).ThenInclude(i => i.FuelType)
            .Include(r => r.RecordedBy)
            .Where(r => r.InventoryItem.OrganizationId == orgId)
            .OrderByDescending(r => r.RefillDate)
            .Take(count)
            .ToListAsync();
        return _mapper.Map<List<RefillRecordResponseDto>>(refills);
    }

    public async Task<RefillRecordResponseDto> RecordRefillAsync(CreateRefillRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var userId = _currentUser.UserId!.Value;

        var item = await _unitOfWork.InventoryItems.Query()
            .Include(i => i.Station)
            .Include(i => i.FuelType)
            .FirstOrDefaultAsync(i => i.Id == request.InventoryItemId && i.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Inventory item not found");

        var newLevel = Math.Min(item.Capacity, item.CurrentLevel + request.Quantity);
        var oldLevel = item.CurrentLevel;
        bool ebmStockUpdated = false;

        // EBM FIRST — update stock in EBM before saving to DB
        var orgSettings = await _unitOfWork.OrganizationSettings.Query()
            .FirstOrDefaultAsync(s => s.OrganizationId == orgId);
        var ebmEnabled = orgSettings?.EBMEnabled == true;

        if (ebmEnabled)
        {
            if (string.IsNullOrEmpty(item.EBMStockId))
                throw new InvalidOperationException(
                    "EBM is enabled but this inventory item has no EBM Stock ID. " +
                    "Please delete and re-create the fuel type to register it with EBM.");

            _logger.LogInformation("Updating EBM stock for inventory {ItemId}: Quantity={Quantity}, StockId={StockId}",
                item.Id, request.Quantity, item.EBMStockId);

            var ebmSuccess = await _ebmService.UpdateStockAsync(orgId, item.EBMStockId, request.Quantity);

            if (!ebmSuccess)
                throw new InvalidOperationException(
                    "Failed to update stock in EBM. Refill was NOT recorded. Please try again.");

            _logger.LogInformation("EBM stock update succeeded for inventory {ItemId}", item.Id);
            ebmStockUpdated = true;
        }

        // EBM succeeded (or not needed) — now save to DB
        try
        {
            var refill = new RefillRecord
            {
                InventoryItemId = item.Id,
                Quantity = request.Quantity,
                UnitCost = request.UnitCost,
                TotalCost = request.Quantity * request.UnitCost,
                SupplierName = request.SupplierName,
                InvoiceNumber = request.InvoiceNumber,
                RefillDate = request.RefillDate,
                RecordedById = userId
            };

            await _unitOfWork.RefillRecords.AddAsync(refill);

            item.CurrentLevel = newLevel;
            item.LastRefillDate = request.RefillDate;
            _unitOfWork.InventoryItems.Update(item);

            await _unitOfWork.SaveChangesAsync();

            // Reload for mapping
            var saved = await _unitOfWork.RefillRecords.Query()
                .Include(r => r.InventoryItem).ThenInclude(i => i.Station)
                .Include(r => r.InventoryItem).ThenInclude(i => i.FuelType)
                .Include(r => r.RecordedBy)
                .FirstAsync(r => r.Id == refill.Id);

            await _audit.LogAsync("Refill", "InventoryItem", item.Id.ToString(), new
            {
                Station = item.Station.Name, FuelType = item.FuelType.Name,
                OldLevel = oldLevel, NewLevel = newLevel, Quantity = request.Quantity,
                UnitCost = request.UnitCost, TotalCost = request.Quantity * request.UnitCost,
                Supplier = request.SupplierName, Invoice = request.InvoiceNumber,
                EBMStockUpdated = ebmStockUpdated
            });

            _ = _notificationService.NotifyDataChangedAsync(orgId, NotificationConstants.InventoryChanged);
            return _mapper.Map<RefillRecordResponseDto>(saved);
        }
        catch (Exception ex)
        {
            // DB failed — EBM stock was already added and cannot be subtracted via API
            if (ebmStockUpdated)
            {
                _logger.LogError("CRITICAL: DB save failed but EBM stock already increased by {Quantity} for inventory {ItemId} (StockId={StockId}). EBM stock addition cannot be reverted. Manual fix required.",
                    request.Quantity, item.Id, item.EBMStockId);
            }

            await _audit.LogAsync("RefillFailed", "InventoryItem", item.Id.ToString(), new
            {
                Station = item.Station.Name, FuelType = item.FuelType.Name,
                Quantity = request.Quantity, EBMStockUpdated = ebmStockUpdated,
                Error = ex.Message
            });

            throw new InvalidOperationException(
                $"Failed to save refill to database. EBM stock was already updated — please contact support. Error: {ex.Message}");
        }
    }

    public async Task UpdateReorderLevelAsync(UpdateReorderLevelRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var item = await _unitOfWork.InventoryItems.Query()
            .FirstOrDefaultAsync(i => i.Id == request.Id && i.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Inventory item not found");

        item.ReorderLevel = request.ReorderLevel;
        _unitOfWork.InventoryItems.Update(item);
        await _unitOfWork.SaveChangesAsync();
    }
}
