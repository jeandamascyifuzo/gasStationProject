using Escale.API.DTOs.Sales;

namespace Escale.API.Services.Interfaces;

public interface ISaleService
{
    Task<SaleResponseDto> CreateSaleAsync(CreateSaleRequestDto request);
    Task<List<DTOs.Transactions.TransactionResponseDto>> GetRecentSalesAsync(Guid? stationId, int count = 10);
}
