using Escale.API.DTOs.EBM;

namespace Escale.API.Services.Interfaces;

public interface IEBMService
{
    Task<EBMSellResult> SendSaleReceiptAsync(Guid orgId, string variantId, decimal qty,
        string? customerName, string? customerPhone);
    Task<bool> UpdatePriceAsync(Guid orgId, string variantId, decimal retailPrice, decimal supplyPrice);
    Task<bool> UpdateStockAsync(Guid orgId, string stockId, decimal currentStock);
    Task<bool> TestConnectionAsync(Guid orgId);
    Task<EBMCreateProductResult> CreateProductAsync(Guid orgId, string productName, decimal retailPrice, decimal supplyPrice);
    Task<bool> DeleteProductAsync(Guid orgId, string productId);
}
