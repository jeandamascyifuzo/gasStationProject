using Escale.API.DTOs.Stock;

namespace Escale.API.Services.Interfaces;

public interface IStockService
{
    Task<List<StockLevelDto>> GetStockLevelsAsync(Guid? stationId = null);
    Task RecordRefillAsync(Guid stationId, string fuelType, decimal quantity, decimal unitCost, string? supplierName, string? invoiceNumber, DateTime refillDate);
}
