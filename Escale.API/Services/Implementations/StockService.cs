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

    public StockService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
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
        item.CurrentLevel = Math.Min(item.Capacity, item.CurrentLevel + quantity);
        item.LastRefillDate = refillDate;
        _unitOfWork.InventoryItems.Update(item);
        await _unitOfWork.SaveChangesAsync();
    }
}
