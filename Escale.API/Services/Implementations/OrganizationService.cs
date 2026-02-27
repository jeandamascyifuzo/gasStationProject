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

    public OrganizationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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

        _unitOfWork.OrganizationSettings.Update(settings);
        await _unitOfWork.SaveChangesAsync();
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

        var ft = new FuelType
        {
            OrganizationId = orgId,
            Name = request.Name,
            CurrentPrice = request.PricePerLiter,
            IsActive = true
        };
        await _unitOfWork.FuelTypes.AddAsync(ft);

        await _unitOfWork.FuelPrices.AddAsync(new FuelPrice
        {
            FuelTypeId = ft.Id,
            Price = request.PricePerLiter,
            EffectiveFrom = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<FuelTypeResponseDto>(ft);
    }

    public async Task<FuelTypeResponseDto> UpdateOrganizationFuelTypeAsync(Guid orgId, Guid fuelTypeId, UpdateFuelTypeRequestDto request)
    {
        var ft = await _unitOfWork.FuelTypes.Query()
            .FirstOrDefaultAsync(f => f.Id == fuelTypeId && f.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Fuel type not found");

        ft.Name = request.Name;
        ft.CurrentPrice = request.PricePerLiter;
        ft.IsActive = request.IsActive;

        _unitOfWork.FuelTypes.Update(ft);
        await _unitOfWork.SaveChangesAsync();

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
