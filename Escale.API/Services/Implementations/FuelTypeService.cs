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

    public FuelTypeService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
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

        var fuelType = new FuelType
        {
            OrganizationId = orgId,
            Name = request.Name,
            CurrentPrice = request.PricePerLiter,
            IsActive = true
        };

        await _unitOfWork.FuelTypes.AddAsync(fuelType);

        // Record initial price
        await _unitOfWork.FuelPrices.AddAsync(new FuelPrice
        {
            FuelTypeId = fuelType.Id,
            Price = request.PricePerLiter,
            EffectiveFrom = DateTime.UtcNow
        });

        // Create inventory items for all active stations
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
                ReorderLevel = 5000
            });
        }

        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<FuelTypeResponseDto>(fuelType);
    }

    public async Task<FuelTypeResponseDto> UpdateFuelTypeAsync(Guid id, UpdateFuelTypeRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var fuelType = await _unitOfWork.FuelTypes.Query()
            .FirstOrDefaultAsync(f => f.Id == id && f.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Fuel type not found");

        // If price changed, record in price history
        if (fuelType.CurrentPrice != request.PricePerLiter)
        {
            // Close previous price period
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
        _unitOfWork.FuelTypes.Update(fuelType);
        await _unitOfWork.SaveChangesAsync();
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
