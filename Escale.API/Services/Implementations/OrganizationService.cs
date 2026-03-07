using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Constants;
using Escale.API.Domain.Entities;
using Escale.API.Domain.Enums;
using Escale.API.DTOs.FuelTypes;
using Escale.API.DTOs.Organizations;
using Escale.API.DTOs.Settings;
using Escale.API.DTOs.Stations;
using Escale.API.DTOs.Users;
using Escale.API.Hubs;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class OrganizationService : IOrganizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEBMService _ebmService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrganizationService> _logger;

    public OrganizationService(IUnitOfWork unitOfWork, IMapper mapper,
        IEBMService ebmService, INotificationService notificationService, ILogger<OrganizationService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _ebmService = ebmService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<List<OrganizationResponseDto>> GetAllOrganizationsAsync()
    {
        var orgs = await _unitOfWork.Organizations.Query()
            .IgnoreQueryFilters()
            .Include(o => o.Stations.Where(s => !s.IsDeleted))
            .Include(o => o.Users.Where(u => !u.IsDeleted))
            .Where(o => o.Slug != "system")
            .OrderBy(o => o.IsDeleted)
            .ThenBy(o => o.Name)
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
            IsDeleted = o.IsDeleted,
            DeletedAt = o.DeletedAt,
            CreatedAt = o.CreatedAt,
            StationCount = o.Stations.Count,
            UserCount = o.Users.Count
        }).ToList();
    }

    public async Task<OrganizationResponseDto> GetOrganizationByIdAsync(Guid id)
    {
        var org = await _unitOfWork.Organizations.Query()
            .IgnoreQueryFilters()
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
            IsDeleted = org.IsDeleted,
            DeletedAt = org.DeletedAt,
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
            UserCount = 0
        };
    }

    public async Task<OrganizationResponseDto> UpdateOrganizationAsync(Guid id, UpdateOrganizationRequestDto request)
    {
        await EnsureOrganizationNotDeleted(id);
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

        org.IsDeleted = true;
        org.IsActive = false;
        org.DeletedAt = DateTime.UtcNow;
        _unitOfWork.Organizations.Update(org);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RestoreOrganizationAsync(Guid id)
    {
        var org = await _unitOfWork.Organizations.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException("Organization not found");

        if (!org.IsDeleted)
            throw new InvalidOperationException("Organization is not deleted");

        org.IsDeleted = false;
        org.IsActive = true;
        org.DeletedAt = null;
        _unitOfWork.Organizations.Update(org);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task EnsureOrganizationNotDeleted(Guid orgId)
    {
        var org = await _unitOfWork.Organizations.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == orgId);

        if (org == null)
            throw new KeyNotFoundException("Organization not found");
        if (org.IsDeleted)
            throw new InvalidOperationException("Cannot perform this action on a deleted organization. Restore it first.");
    }

    public async Task<List<StationResponseDto>> GetOrganizationStationsAsync(Guid orgId)
    {
        var stations = await _unitOfWork.Stations.Query()
            .Include(s => s.Manager)
            .Where(s => s.OrganizationId == orgId)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return _mapper.Map<List<StationResponseDto>>(stations);
    }

    public async Task ToggleOrganizationStationStatusAsync(Guid orgId, Guid stationId)
    {
        await EnsureOrganizationNotDeleted(orgId);
        var station = await _unitOfWork.Stations.Query()
            .FirstOrDefaultAsync(s => s.Id == stationId && s.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Station not found");

        station.IsActive = !station.IsActive;
        _unitOfWork.Stations.Update(station);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<StationResponseDto> CreateOrganizationStationAsync(Guid orgId, CreateStationRequestDto request)
    {
        await EnsureOrganizationNotDeleted(orgId);
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
        await EnsureOrganizationNotDeleted(orgId);
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
        await EnsureOrganizationNotDeleted(orgId);
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
        await EnsureOrganizationNotDeleted(orgId);
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
        await EnsureOrganizationNotDeleted(orgId);
        var ft = await _unitOfWork.FuelTypes.Query()
            .FirstOrDefaultAsync(f => f.Id == fuelTypeId && f.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Fuel type not found");

        _unitOfWork.FuelTypes.Remove(ft);
        await _unitOfWork.SaveChangesAsync();
        _ = _notificationService.NotifyDataChangedAsync(orgId, NotificationConstants.FuelTypesChanged);
    }

    public async Task<List<FuelTypeResponseDto>> GetDeletedOrganizationFuelTypesAsync(Guid orgId)
    {
        var fuelTypes = await _unitOfWork.FuelTypes.Query()
            .IgnoreQueryFilters()
            .Where(f => f.OrganizationId == orgId && f.IsDeleted)
            .OrderBy(f => f.Name)
            .ToListAsync();

        return _mapper.Map<List<FuelTypeResponseDto>>(fuelTypes);
    }

    public async Task RestoreOrganizationFuelTypeAsync(Guid orgId, Guid fuelTypeId)
    {
        await EnsureOrganizationNotDeleted(orgId);
        var ft = await _unitOfWork.FuelTypes.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == fuelTypeId && f.OrganizationId == orgId && f.IsDeleted)
            ?? throw new KeyNotFoundException("Deleted fuel type not found");

        ft.IsDeleted = false;
        ft.DeletedAt = null;
        _unitOfWork.FuelTypes.Update(ft);
        await _unitOfWork.SaveChangesAsync();
        _ = _notificationService.NotifyDataChangedAsync(orgId, NotificationConstants.FuelTypesChanged);
    }

    public async Task<UserResponseDto?> GetOrganizationAdminAsync(Guid orgId)
    {
        var admin = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.OrganizationId == orgId && u.Role == UserRole.Admin);

        if (admin == null) return null;

        return new UserResponseDto
        {
            Id = admin.Id,
            Username = admin.Username,
            FullName = admin.FullName,
            Email = admin.Email,
            Phone = admin.Phone,
            Role = admin.Role.ToString(),
            IsActive = admin.IsActive,
            LastLoginAt = admin.LastLoginAt,
            CreatedAt = admin.CreatedAt
        };
    }

    public async Task<UserResponseDto> CreateOrganizationAdminAsync(Guid orgId, CreateOrgAdminRequestDto request)
    {
        await EnsureOrganizationNotDeleted(orgId);
        var org = await _unitOfWork.Organizations.GetByIdAsync(orgId)
            ?? throw new KeyNotFoundException("Organization not found");

        var existingAdmin = await _unitOfWork.Users.Query()
            .AnyAsync(u => u.OrganizationId == orgId && u.Role == UserRole.Admin);
        if (existingAdmin)
            throw new InvalidOperationException("This organization already has an admin user");

        var usernameExists = await _unitOfWork.Users.ExistsAsync(u => u.Username == request.Username);
        if (usernameExists)
            throw new InvalidOperationException("Username already exists");

        var admin = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(admin);
        await _unitOfWork.SaveChangesAsync();

        return new UserResponseDto
        {
            Id = admin.Id,
            Username = admin.Username,
            FullName = admin.FullName,
            Email = admin.Email,
            Phone = admin.Phone,
            Role = admin.Role.ToString(),
            IsActive = admin.IsActive,
            CreatedAt = admin.CreatedAt
        };
    }
}
