using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Entities;
using Escale.API.DTOs.Inventory;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public InventoryService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<List<InventoryItemResponseDto>> GetInventoryAsync(Guid? stationId = null)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var query = _unitOfWork.InventoryItems.Query()
            .Include(i => i.Station)
            .Include(i => i.FuelType)
            .Where(i => i.OrganizationId == orgId);

        if (stationId.HasValue)
            query = query.Where(i => i.StationId == stationId.Value);

        var items = await query.OrderBy(i => i.Station.Name).ThenBy(i => i.FuelType.Name).ToListAsync();
        var dtos = _mapper.Map<List<InventoryItemResponseDto>>(items);

        // Set computed status
        foreach (var dto in dtos)
        {
            var pct = dto.PercentageFull / 100;
            dto.Status = pct < 0.10m ? "Critical" : pct < 0.25m ? "Low Stock" : "Normal";
        }

        return dtos;
    }

    public async Task<List<RefillRecordResponseDto>> GetRefillsAsync(int count = 20)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var refills = await _unitOfWork.RefillRecords.Query()
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

        item.CurrentLevel = Math.Min(item.Capacity, item.CurrentLevel + request.Quantity);
        item.LastRefillDate = request.RefillDate;
        _unitOfWork.InventoryItems.Update(item);

        await _unitOfWork.SaveChangesAsync();

        // Reload for mapping
        var saved = await _unitOfWork.RefillRecords.Query()
            .Include(r => r.InventoryItem).ThenInclude(i => i.Station)
            .Include(r => r.InventoryItem).ThenInclude(i => i.FuelType)
            .Include(r => r.RecordedBy)
            .FirstAsync(r => r.Id == refill.Id);

        return _mapper.Map<RefillRecordResponseDto>(saved);
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
