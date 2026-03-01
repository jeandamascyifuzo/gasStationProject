using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Entities;
using Escale.API.DTOs.FuelTypes;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class FuelTypeService : IFuelTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IEBMService _ebmService;
    private readonly ILogger<FuelTypeService> _logger;

    public FuelTypeService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper,
        IEBMService ebmService, ILogger<FuelTypeService> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _ebmService = ebmService;
        _logger = logger;
    }

    public async Task<List<FuelTypeResponseDto>> GetFuelTypesAsync()
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var fuelTypes = await _unitOfWork.FuelTypes.Query()
            .Where(f => f.OrganizationId == orgId)
            .OrderBy(f => f.Name)
            .ToListAsync();
        return _mapper.Map<List<FuelTypeResponseDto>>(fuelTypes);
    }

    public async Task<FuelTypeResponseDto> GetFuelTypeByIdAsync(Guid id)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var fuelType = await _unitOfWork.FuelTypes.Query()
            .FirstOrDefaultAsync(f => f.Id == id && f.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Fuel type not found");
        return _mapper.Map<FuelTypeResponseDto>(fuelType);
    }

    public async Task<FuelTypeResponseDto> CreateFuelTypeAsync(CreateFuelTypeRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;

        if (await _unitOfWork.FuelTypes.ExistsAsync(f => f.OrganizationId == orgId && f.Name == request.Name))
            throw new InvalidOperationException("Fuel type name already exists");

        // EBM FIRST — register product in EBM before saving to DB
        string? ebmProductId = null;
        string? ebmVariantId = request.EBMVariantId;
        string? ebmStockId = null;

        var orgSettings = await _unitOfWork.OrganizationSettings.Query()
            .FirstOrDefaultAsync(s => s.OrganizationId == orgId);

        if (orgSettings?.EBMEnabled == true)
        {
            var ebmResult = await _ebmService.CreateProductAsync(
                orgId, request.Name, request.PricePerLiter, request.EBMSupplyPrice ?? 0);

            if (!ebmResult.Success)
                throw new InvalidOperationException(
                    $"Failed to register product in EBM: {ebmResult.ErrorMessage}. Fuel type was NOT created.");

            ebmProductId = ebmResult.ProductId;
            ebmVariantId = ebmResult.VariantId;
            ebmStockId = ebmResult.StockId;
        }

        // EBM succeeded — now save to DB
        try
        {
            var fuelType = new FuelType
            {
                OrganizationId = orgId,
                Name = request.Name,
                CurrentPrice = request.PricePerLiter,
                IsActive = true,
                EBMProductId = ebmProductId,
                EBMVariantId = ebmVariantId,
                EBMSupplyPrice = request.EBMSupplyPrice
            };

            await _unitOfWork.FuelTypes.AddAsync(fuelType);

            await _unitOfWork.FuelPrices.AddAsync(new FuelPrice
            {
                FuelTypeId = fuelType.Id,
                Price = request.PricePerLiter,
                EffectiveFrom = DateTime.UtcNow
            });

            var stations = await _unitOfWork.Stations.Query()
                .Where(s => s.OrganizationId == orgId && s.IsActive)
                .ToListAsync();

            foreach (var station in stations)
            {
                await _unitOfWork.InventoryItems.AddAsync(new InventoryItem
                {
                    OrganizationId = orgId,
                    StationId = station.Id,
                    FuelTypeId = fuelType.Id,
                    CurrentLevel = 0,
                    Capacity = 20000,
                    ReorderLevel = 5000,
                    EBMStockId = ebmStockId
                });
            }

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<FuelTypeResponseDto>(fuelType);
        }
        catch (Exception ex)
        {
            // DB failed — revert EBM by deleting the product so nothing is out of sync
            if (!string.IsNullOrEmpty(ebmProductId))
            {
                _logger.LogWarning("DB save failed, deleting EBM product {ProductId} for org {OrgId}", ebmProductId, orgId);
                var reverted = await _ebmService.DeleteProductAsync(orgId, ebmProductId);
                if (!reverted)
                    _logger.LogError("CRITICAL: Failed to delete EBM product {ProductId} for org {OrgId}. Product exists in EBM but not in DB. Manual fix required.",
                        ebmProductId, orgId);
            }
            throw new InvalidOperationException(
                $"Failed to save fuel type to database. EBM product has been reverted. Error: {ex.Message}");
        }
    }

    public async Task<FuelTypeResponseDto> UpdateFuelTypeAsync(Guid id, UpdateFuelTypeRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var fuelType = await _unitOfWork.FuelTypes.Query()
            .FirstOrDefaultAsync(f => f.Id == id && f.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Fuel type not found");

        var priceChanged = fuelType.CurrentPrice != request.PricePerLiter;
        var supplyPriceChanged = request.EBMSupplyPrice.HasValue && request.EBMSupplyPrice != fuelType.EBMSupplyPrice;
        var oldPrice = fuelType.CurrentPrice;
        var ebmVariantId = request.EBMVariantId ?? fuelType.EBMVariantId;
        var ebmSupplyPrice = request.EBMSupplyPrice ?? fuelType.EBMSupplyPrice ?? 0;
        var oldSupplyPrice = fuelType.EBMSupplyPrice ?? 0;
        bool ebmPriceUpdated = false;

        // Check if EBM is enabled for this org
        var orgSettings = await _unitOfWork.OrganizationSettings.Query()
            .FirstOrDefaultAsync(s => s.OrganizationId == orgId);
        var ebmEnabled = orgSettings?.EBMEnabled == true;

        // If EBM is enabled and price changed, EBM update is REQUIRED — no partial save
        if ((priceChanged || supplyPriceChanged) && ebmEnabled)
        {
            if (string.IsNullOrEmpty(ebmVariantId))
                throw new InvalidOperationException(
                    "EBM is enabled but this fuel type has no EBM Variant ID. " +
                    "Please delete and re-create the fuel type to register it with EBM.");

            _logger.LogInformation("Updating EBM price for fuel type {FuelTypeId}: RetailPrice={RetailPrice}, SupplyPrice={SupplyPrice}, VariantId={VariantId}",
                id, request.PricePerLiter, ebmSupplyPrice, ebmVariantId);

            var ebmSuccess = await _ebmService.UpdatePriceAsync(orgId, ebmVariantId,
                request.PricePerLiter, ebmSupplyPrice);

            if (!ebmSuccess)
                throw new InvalidOperationException(
                    "Failed to update price in EBM. No changes were saved. Please try again.");

            _logger.LogInformation("EBM price update succeeded for fuel type {FuelTypeId}", id);
            ebmPriceUpdated = true;
        }

        // EBM succeeded (or not needed) — now save to DB
        try
        {
            if (priceChanged)
            {
                var lastPrice = await _unitOfWork.FuelPrices.Query()
                    .Where(fp => fp.FuelTypeId == id && fp.EffectiveTo == null)
                    .FirstOrDefaultAsync();
                if (lastPrice != null)
                {
                    lastPrice.EffectiveTo = DateTime.UtcNow;
                    _unitOfWork.FuelPrices.Update(lastPrice);
                }

                await _unitOfWork.FuelPrices.AddAsync(new FuelPrice
                {
                    FuelTypeId = id,
                    Price = request.PricePerLiter,
                    EffectiveFrom = DateTime.UtcNow
                });

                fuelType.CurrentPrice = request.PricePerLiter;
            }

            fuelType.Name = request.Name;
            fuelType.IsActive = request.IsActive;
            fuelType.EBMVariantId = request.EBMVariantId ?? fuelType.EBMVariantId;
            fuelType.EBMSupplyPrice = request.EBMSupplyPrice ?? fuelType.EBMSupplyPrice;
            _unitOfWork.FuelTypes.Update(fuelType);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // DB failed — revert EBM price back to old values so nothing is out of sync
            if (ebmPriceUpdated)
            {
                _logger.LogWarning("DB save failed, reverting EBM price for fuel type {FuelTypeId} back to RetailPrice={OldPrice}, SupplyPrice={OldSupplyPrice}",
                    id, oldPrice, oldSupplyPrice);
                var reverted = await _ebmService.UpdatePriceAsync(orgId, ebmVariantId!, oldPrice, oldSupplyPrice);
                if (!reverted)
                    _logger.LogError("CRITICAL: Failed to revert EBM price for fuel type {FuelTypeId}. EBM has RetailPrice={NewPrice} but DB has RetailPrice={OldPrice}. Manual fix required.",
                        id, request.PricePerLiter, oldPrice);
            }
            throw new InvalidOperationException(
                $"Failed to save price update to database. EBM has been reverted. Error: {ex.Message}");
        }

        return _mapper.Map<FuelTypeResponseDto>(fuelType);
    }

    public async Task DeleteFuelTypeAsync(Guid id)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var fuelType = await _unitOfWork.FuelTypes.Query()
            .FirstOrDefaultAsync(f => f.Id == id && f.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Fuel type not found");
        _unitOfWork.FuelTypes.Remove(fuelType);
        await _unitOfWork.SaveChangesAsync();
    }
}
