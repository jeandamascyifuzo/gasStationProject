using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Constants;
using Escale.API.Domain.Entities;
using Escale.API.Domain.Enums;
using Escale.API.DTOs.FuelTypes;
using Escale.API.DTOs.Organizations;
using Escale.API.DTOs.Settings;
using Escale.API.DTOs.Stations;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class OrganizationService : IOrganizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEBMService _ebmService;
    private readonly ILogger<OrganizationService> _logger;

    public OrganizationService(IUnitOfWork unitOfWork, IMapper mapper,
        IEBMService ebmService, ILogger<OrganizationService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _ebmService = ebmService;
        _logger = logger;
    }

    public async Task<List<OrganizationResponseDto>> GetAllOrganizationsAsync()
    {
        var orgs = await _unitOfWork.Organizations.Query()
            .Include(o => o.Stations.Where(s => !s.IsDeleted))
            .Include(o => o.Users.Where(u => !u.IsDeleted))
            .Where(o => o.Slug != "system")
            .OrderBy(o => o.Name)
            .ToListAsync();

        return orgs.Select(o => new OrganizationResponseDto
        {
            Id = o.Id,
            Name = o.Name,
            Slug = o.Slug,
            TIN = o.TIN,
            Address = o.Address,
            Phone = o.Phone,
            Email = o.Email,
            IsActive = o.IsActive,
            CreatedAt = o.CreatedAt,
            StationCount = o.Stations.Count,
            UserCount = o.Users.Count
        }).ToList();
    }

    public async Task<OrganizationResponseDto> GetOrganizationByIdAsync(Guid id)
    {
        var org = await _unitOfWork.Organizations.Query()
            .Include(o => o.Stations.Where(s => !s.IsDeleted))
            .Include(o => o.Users.Where(u => !u.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException("Organization not found");

        return new OrganizationResponseDto
        {
            Id = org.Id,
            Name = org.Name,
            Slug = org.Slug,
            TIN = org.TIN,
            Address = org.Address,
            Phone = org.Phone,
            Email = org.Email,
            IsActive = org.IsActive,
            CreatedAt = org.CreatedAt,
            StationCount = org.Stations.Count,
            UserCount = org.Users.Count
        };
    }

    public async Task<OrganizationResponseDto> CreateOrganizationAsync(CreateOrganizationRequestDto request)
    {
        var exists = await _unitOfWork.Organizations.ExistsAsync(o => o.Name == request.Name);
        if (exists)
            throw new InvalidOperationException("Organization name already exists");

        var orgId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var slug = request.Name.ToLower().Replace(" ", "-");

        var org = new Organization
        {
            Id = orgId,
            Name = request.Name,
            Slug = slug,
            TIN = request.TIN,
            Address = request.Address,
            Phone = request.Phone,
            Email = request.Email,
            IsActive = true,
            CreatedAt = now
        };
        await _unitOfWork.Organizations.AddAsync(org);

        // Create default admin user
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            Username = $"admin-{slug}",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            FullName = $"{request.Name} Admin",
            Email = request.Email,
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = now
        };
        await _unitOfWork.Users.AddAsync(adminUser);

        // Default fuel types
        var fuelTypes = new[]
        {
            new FuelType { Id = Guid.NewGuid(), OrganizationId = orgId, Name = "Petrol 95", CurrentPrice = 1450m, IsActive = true, CreatedAt = now },
            new FuelType { Id = Guid.NewGuid(), OrganizationId = orgId, Name = "Diesel", CurrentPrice = 1380m, IsActive = true, CreatedAt = now }
        };
        foreach (var ft in fuelTypes)
        {
            await _unitOfWork.FuelTypes.AddAsync(ft);
            await _unitOfWork.FuelPrices.AddAsync(new FuelPrice
            {
                Id = Guid.NewGuid(), FuelTypeId = ft.Id, Price = ft.CurrentPrice,
                EffectiveFrom = now, CreatedAt = now
            });
        }

        // Default settings
        await _unitOfWork.OrganizationSettings.AddAsync(new OrganizationSettings
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            CompanyName = request.Name,
            TaxRate = BusinessRules.VATRate,
            Currency = BusinessRules.DefaultCurrency,
            ReceiptHeader = $"{request.Name}\n{request.Address ?? "Rwanda"}",
            ReceiptFooter = BusinessRules.DefaultReceiptFooter,
            EBMEnabled = false,
            MinimumSaleAmount = BusinessRules.DefaultMinimumSaleAmount,
            MaximumSaleAmount = BusinessRules.DefaultMaximumSaleAmount,
            LowStockThreshold = 0.20m,
            CriticalStockThreshold = 0.10m,
            CreatedAt = now
        });

        await _unitOfWork.SaveChangesAsync();

        return new OrganizationResponseDto
        {
            Id = orgId,
            Name = org.Name,
            Slug = org.Slug,
            TIN = org.TIN,
            Address = org.Address,
            Phone = org.Phone,
            Email = org.Email,
            IsActive = org.IsActive,
            CreatedAt = org.CreatedAt,
            StationCount = 0,
            UserCount = 1
        };
    }

    public async Task<OrganizationResponseDto> UpdateOrganizationAsync(Guid id, UpdateOrganizationRequestDto request)
    {
        var org = await _unitOfWork.Organizations.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Organization not found");

        org.Name = request.Name;
        org.TIN = request.TIN;
        org.Address = request.Address;
        org.Phone = request.Phone;
        org.Email = request.Email;
        org.IsActive = request.IsActive;

        _unitOfWork.Organizations.Update(org);
        await _unitOfWork.SaveChangesAsync();

        return await GetOrganizationByIdAsync(id);
    }

    public async Task DeleteOrganizationAsync(Guid id)
    {
        var org = await _unitOfWork.Organizations.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Organization not found");

        if (org.Slug == "system")
            throw new InvalidOperationException("Cannot delete the system organization");

        _unitOfWork.Organizations.Remove(org);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<StationResponseDto>> GetOrganizationStationsAsync(Guid orgId)
    {
        var stations = await _unitOfWork.Stations.Query()
            .Include(s => s.Manager)
            .Where(s => s.OrganizationId == orgId)
            .ToListAsync();

        return _mapper.Map<List<StationResponseDto>>(stations);
    }

    public async Task<StationResponseDto> CreateOrganizationStationAsync(Guid orgId, CreateStationRequestDto request)
    {
        var org = await _unitOfWork.Organizations.GetByIdAsync(orgId)
            ?? throw new KeyNotFoundException("Organization not found");

        var station = _mapper.Map<Station>(request);
        station.OrganizationId = orgId;
        station.IsActive = true;

        await _unitOfWork.Stations.AddAsync(station);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<StationResponseDto>(station);
    }

    public async Task ConfigureEbmAsync(Guid orgId, EbmConfigRequestDto request)
    {
        var settings = await _unitOfWork.OrganizationSettings.Query()
            .FirstOrDefaultAsync(s => s.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Organization settings not found");

        settings.EBMEnabled = request.EBMEnabled;
        settings.EBMServerUrl = request.EBMServerUrl;
        settings.EBMBusinessId = request.EBMBusinessId;
        settings.EBMBranchId = request.EBMBranchId;
        settings.EBMCompanyName = request.EBMCompanyName;
        settings.EBMCompanyAddress = request.EBMCompanyAddress;
        settings.EBMCompanyPhone = request.EBMCompanyPhone;
        settings.EBMCompanyTIN = request.EBMCompanyTIN;
        settings.EBMCategoryId = request.EBMCategoryId;

        _unitOfWork.OrganizationSettings.Update(settings);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<EbmConfigResponseDto> GetEbmConfigAsync(Guid orgId)
    {
        var settings = await _unitOfWork.OrganizationSettings.Query()
            .FirstOrDefaultAsync(s => s.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Organization settings not found");

        return new EbmConfigResponseDto
        {
            EBMEnabled = settings.EBMEnabled,
            EBMServerUrl = settings.EBMServerUrl,
            EBMBusinessId = settings.EBMBusinessId,
            EBMBranchId = settings.EBMBranchId,
            EBMCompanyName = settings.EBMCompanyName,
            EBMCompanyAddress = settings.EBMCompanyAddress,
            EBMCompanyPhone = settings.EBMCompanyPhone,
            EBMCompanyTIN = settings.EBMCompanyTIN,
            EBMCategoryId = settings.EBMCategoryId,
            IsConfigured = !string.IsNullOrEmpty(settings.EBMBusinessId) && !string.IsNullOrEmpty(settings.EBMBranchId)
        };
    }

    public async Task<List<FuelTypeResponseDto>> GetOrganizationFuelTypesAsync(Guid orgId)
    {
        var fuelTypes = await _unitOfWork.FuelTypes.Query()
            .Where(f => f.OrganizationId == orgId)
            .OrderBy(f => f.Name)
            .ToListAsync();

        return _mapper.Map<List<FuelTypeResponseDto>>(fuelTypes);
    }

    public async Task<FuelTypeResponseDto> CreateOrganizationFuelTypeAsync(Guid orgId, CreateFuelTypeRequestDto request)
    {
        var org = await _unitOfWork.Organizations.GetByIdAsync(orgId)
            ?? throw new KeyNotFoundException("Organization not found");

        var exists = await _unitOfWork.FuelTypes.ExistsAsync(f => f.OrganizationId == orgId && f.Name == request.Name);
        if (exists)
            throw new InvalidOperationException("Fuel type already exists for this organization");

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
            var ft = new FuelType
            {
                OrganizationId = orgId,
                Name = request.Name,
                CurrentPrice = request.PricePerLiter,
                IsActive = true,
                EBMProductId = ebmProductId,
                EBMVariantId = ebmVariantId,
                EBMSupplyPrice = request.EBMSupplyPrice
            };
            await _unitOfWork.FuelTypes.AddAsync(ft);

            await _unitOfWork.FuelPrices.AddAsync(new FuelPrice
            {
                FuelTypeId = ft.Id,
                Price = request.PricePerLiter,
                EffectiveFrom = DateTime.UtcNow
            });

            // Create inventory items with EBM stock ID for all active stations
            var stations = await _unitOfWork.Stations.Query()
                .Where(s => s.OrganizationId == orgId && s.IsActive)
                .ToListAsync();

            foreach (var station in stations)
            {
                await _unitOfWork.InventoryItems.AddAsync(new InventoryItem
                {
                    OrganizationId = orgId,
                    StationId = station.Id,
                    FuelTypeId = ft.Id,
                    CurrentLevel = 0,
                    Capacity = 20000,
                    ReorderLevel = 5000,
                    EBMStockId = ebmStockId
                });
            }

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<FuelTypeResponseDto>(ft);
        }
        catch (Exception ex)
        {
            // DB failed — revert EBM by deleting the product
            if (!string.IsNullOrEmpty(ebmProductId))
            {
                _logger.LogWarning("DB save failed, deleting EBM product {ProductId} for org {OrgId}", ebmProductId, orgId);
                await _ebmService.DeleteProductAsync(orgId, ebmProductId);
            }
            throw new InvalidOperationException(
                $"Failed to save fuel type to database. EBM product has been reverted. Error: {ex.Message}");
        }
    }

    public async Task<FuelTypeResponseDto> UpdateOrganizationFuelTypeAsync(Guid orgId, Guid fuelTypeId, UpdateFuelTypeRequestDto request)
    {
        var ft = await _unitOfWork.FuelTypes.Query()
            .FirstOrDefaultAsync(f => f.Id == fuelTypeId && f.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Fuel type not found");

        var priceChanged = ft.CurrentPrice != request.PricePerLiter;
        var oldPrice = ft.CurrentPrice;
        var ebmVariantId = request.EBMVariantId ?? ft.EBMVariantId;
        var ebmSupplyPrice = request.EBMSupplyPrice ?? ft.EBMSupplyPrice ?? 0;
        bool ebmPriceUpdated = false;

        // EBM FIRST — sync new price to EBM before saving to DB
        if (priceChanged && !string.IsNullOrEmpty(ebmVariantId))
        {
            var orgSettings = await _unitOfWork.OrganizationSettings.Query()
                .FirstOrDefaultAsync(s => s.OrganizationId == orgId);

            if (orgSettings?.EBMEnabled == true)
            {
                var ebmSuccess = await _ebmService.UpdatePriceAsync(orgId, ebmVariantId,
                    request.PricePerLiter, ebmSupplyPrice);

                if (!ebmSuccess)
                    throw new InvalidOperationException(
                        "Failed to update price in EBM. Price was NOT changed. Please try again.");

                ebmPriceUpdated = true;
            }
        }

        // EBM succeeded — now save to DB
        try
        {
            ft.Name = request.Name;
            ft.CurrentPrice = request.PricePerLiter;
            ft.IsActive = request.IsActive;
            ft.EBMVariantId = request.EBMVariantId;
            ft.EBMSupplyPrice = request.EBMSupplyPrice;

            _unitOfWork.FuelTypes.Update(ft);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // DB failed — revert EBM price back to old value
            if (ebmPriceUpdated)
            {
                _logger.LogWarning("DB save failed, reverting EBM price for fuel type {FuelTypeId} back to {OldPrice}", fuelTypeId, oldPrice);
                await _ebmService.UpdatePriceAsync(orgId, ebmVariantId!, oldPrice, ebmSupplyPrice);
            }
            throw new InvalidOperationException(
                $"Failed to save price update to database. EBM has been reverted. Error: {ex.Message}");
        }

        return _mapper.Map<FuelTypeResponseDto>(ft);
    }

    public async Task DeleteOrganizationFuelTypeAsync(Guid orgId, Guid fuelTypeId)
    {
        var ft = await _unitOfWork.FuelTypes.Query()
            .FirstOrDefaultAsync(f => f.Id == fuelTypeId && f.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Fuel type not found");

        _unitOfWork.FuelTypes.Remove(ft);
        await _unitOfWork.SaveChangesAsync();
    }
}
