using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Entities;
using Escale.API.DTOs.Stations;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class StationService : IStationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public StationService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<List<StationResponseDto>> GetStationsAsync()
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var stations = await _unitOfWork.Stations.Query()
            .Include(s => s.Manager)
            .Where(s => s.OrganizationId == orgId)
            .OrderBy(s => s.Name)
            .ToListAsync();
        return _mapper.Map<List<StationResponseDto>>(stations);
    }

    public async Task<StationDetailResponseDto> GetStationByIdAsync(Guid id)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var station = await _unitOfWork.Stations.Query()
            .Include(s => s.Manager)
            .Include(s => s.UserStations)
            .Include(s => s.Transactions.Where(t => t.TransactionDate.Date == DateTime.UtcNow.Date))
            .FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Station not found");

        var dto = _mapper.Map<StationDetailResponseDto>(station);
        dto.EmployeeCount = station.UserStations.Count;
        dto.TodayTransactionCount = station.Transactions.Count;
        dto.TodaySales = station.Transactions.Sum(t => t.Total);
        return dto;
    }

    public async Task<StationResponseDto> CreateStationAsync(CreateStationRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;

        if (await _unitOfWork.Stations.ExistsAsync(s => s.OrganizationId == orgId && s.Name == request.Name))
            throw new InvalidOperationException("Station name already exists");

        var station = _mapper.Map<Station>(request);
        station.OrganizationId = orgId;
        station.IsActive = true;

        await _unitOfWork.Stations.AddAsync(station);
        await _unitOfWork.SaveChangesAsync();

        // Create inventory items for all fuel types
        var fuelTypes = await _unitOfWork.FuelTypes.Query()
            .Where(f => f.OrganizationId == orgId && f.IsActive)
            .ToListAsync();

        foreach (var ft in fuelTypes)
        {
            await _unitOfWork.InventoryItems.AddAsync(new InventoryItem
            {
                OrganizationId = orgId,
                StationId = station.Id,
                FuelTypeId = ft.Id,
                CurrentLevel = 0,
                Capacity = 20000,
                ReorderLevel = 5000
            });
        }
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<StationResponseDto>(station);
    }

    public async Task<StationResponseDto> UpdateStationAsync(Guid id, UpdateStationRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var station = await _unitOfWork.Stations.Query()
            .Include(s => s.Manager)
            .FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Station not found");

        station.Name = request.Name;
        station.Location = request.Location;
        station.Address = request.Address;
        station.PhoneNumber = request.PhoneNumber;
        station.IsActive = request.IsActive;
        station.ManagerId = request.ManagerId;

        _unitOfWork.Stations.Update(station);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<StationResponseDto>(station);
    }

    public async Task DeleteStationAsync(Guid id)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var station = await _unitOfWork.Stations.Query()
            .FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Station not found");
        _unitOfWork.Stations.Remove(station);
        await _unitOfWork.SaveChangesAsync();
    }
}
