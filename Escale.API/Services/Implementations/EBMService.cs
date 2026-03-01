using System.Text;
using System.Text.Json;
using Escale.API.Data.Repositories;
using Escale.API.DTOs.EBM;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class EBMService : IEBMService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EBMService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public EBMService(IHttpClientFactory httpClientFactory, IUnitOfWork unitOfWork, ILogger<EBMService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    private const int MaxInvoiceRetries = 10;

    public async Task<EBMSellResult> SendSaleReceiptAsync(Guid orgId, string variantId, decimal qty,
        string? customerName, string? customerPhone)
    {
        try
        {
            var settings = await GetSettings(orgId);
            if (settings == null || !settings.EBMEnabled || string.IsNullOrEmpty(settings.EBMServerUrl))
                return new EBMSellResult { Success = false, ErrorMessage = "EBM not configured" };

            var payload = new EBMSellPayload
            {
                Items = new List<EBMSellItem> { new() { VariantId = variantId, Qty = qty } },
                CustomerName = !string.IsNullOrEmpty(customerName) ? customerName : "Walk-in",
                ClientId = settings.EBMBranchId,
                CompanyName = settings.EBMCompanyName,
                CompanyAddress = settings.EBMCompanyAddress,
                CompanyPhone = settings.EBMCompanyPhone,
                CompanyTin = settings.EBMCompanyTIN,
                CustomerPhone = !string.IsNullOrEmpty(customerPhone) ? customerPhone : "N/A"
            };

            var client = _httpClientFactory.CreateClient("EBM");
            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            var url = $"{settings.EBMServerUrl.TrimEnd('/')}/receipts/sell";

            // Retry loop for "Invoice number already exists" (YegoBox bug — resultCd 924)
            for (int attempt = 1; attempt <= MaxInvoiceRetries; attempt++)
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var receiptCode = TryExtractReceiptCode(responseBody);
                    _logger.LogInformation("EBM sale receipt sent successfully for org {OrgId}, receipt: {Code} (attempt {Attempt})",
                        orgId, receiptCode, attempt);
                    return new EBMSellResult
                    {
                        Success = true,
                        ReceiptCode = receiptCode,
                        RawResponse = responseBody
                    };
                }

                // Check if it's the "Invoice number already exists" error — retry
                if (IsInvoiceDuplicateError(responseBody))
                {
                    _logger.LogWarning("EBM invoice duplicate for org {OrgId}, attempt {Attempt}/{Max} — retrying...",
                        orgId, attempt, MaxInvoiceRetries);
                    await Task.Delay(500 * attempt); // increasing delay: 500ms, 1s, 1.5s...
                    continue;
                }

                // Any other error — fail immediately
                _logger.LogWarning("EBM sale receipt failed for org {OrgId}: {StatusCode} - {Body}",
                    orgId, response.StatusCode, responseBody);
                return new EBMSellResult
                {
                    Success = false,
                    ErrorMessage = $"EBM API returned {(int)response.StatusCode}: {responseBody}",
                    RawResponse = responseBody
                };
            }

            _logger.LogError("EBM sale receipt failed after {Max} retries (invoice duplicate) for org {OrgId}", MaxInvoiceRetries, orgId);
            return new EBMSellResult
            {
                Success = false,
                ErrorMessage = $"EBM failed after {MaxInvoiceRetries} retries: Invoice number conflict"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EBM sale receipt exception for org {OrgId}", orgId);
            return new EBMSellResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> UpdatePriceAsync(Guid orgId, string variantId, decimal retailPrice, decimal supplyPrice)
    {
        try
        {
            var settings = await GetSettings(orgId);
            if (settings == null || !settings.EBMEnabled || string.IsNullOrEmpty(settings.EBMServerUrl))
                return false;

            var payload = new EBMUpdatePricePayload
            {
                VariantId = variantId,
                RetailPrice = retailPrice,
                SupplyPrice = supplyPrice
            };

            var client = _httpClientFactory.CreateClient("EBM");
            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{settings.EBMServerUrl.TrimEnd('/')}/products/update-price", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("EBM price updated for org {OrgId}, variant {VariantId}", orgId, variantId);
                return true;
            }

            var body = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("EBM price update failed for org {OrgId}: {StatusCode} - {Body}",
                orgId, response.StatusCode, body);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EBM price update exception for org {OrgId}", orgId);
            return false;
        }
    }

    public async Task<bool> UpdateStockAsync(Guid orgId, string stockId, decimal currentStock)
    {
        try
        {
            var settings = await GetSettings(orgId);
            if (settings == null || !settings.EBMEnabled || string.IsNullOrEmpty(settings.EBMServerUrl))
                return false;

            var payload = new EBMUpdateStockPayload
            {
                StockId = stockId,
                CurrentStock = currentStock
            };

            var client = _httpClientFactory.CreateClient("EBM");
            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{settings.EBMServerUrl.TrimEnd('/')}/products/update-stock", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("EBM stock updated for org {OrgId}, stock {StockId}", orgId, stockId);
                return true;
            }

            var body = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("EBM stock update failed for org {OrgId}: {StatusCode} - {Body}",
                orgId, response.StatusCode, body);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EBM stock update exception for org {OrgId}", orgId);
            return false;
        }
    }

    public async Task<bool> TestConnectionAsync(Guid orgId)
    {
        try
        {
            var settings = await GetSettings(orgId);
            if (settings == null || !settings.EBMEnabled || string.IsNullOrEmpty(settings.EBMServerUrl))
                return false;

            var client = _httpClientFactory.CreateClient("EBM");
            var response = await client.GetAsync(settings.EBMServerUrl.TrimEnd('/'));
            _logger.LogInformation("EBM connection test for org {OrgId}: {StatusCode}", orgId, response.StatusCode);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EBM connection test failed for org {OrgId}", orgId);
            return false;
        }
    }

    public async Task<EBMCreateProductResult> CreateProductAsync(Guid orgId, string productName, decimal retailPrice, decimal supplyPrice)
    {
        try
        {
            var settings = await GetSettings(orgId);
            if (settings == null || !settings.EBMEnabled || string.IsNullOrEmpty(settings.EBMServerUrl))
                return new EBMCreateProductResult { Success = false, ErrorMessage = "EBM not configured" };

            if (string.IsNullOrEmpty(settings.EBMBusinessId) || string.IsNullOrEmpty(settings.EBMBranchId))
                return new EBMCreateProductResult { Success = false, ErrorMessage = "EBM Business ID or Branch ID not configured" };

            if (string.IsNullOrEmpty(settings.EBMCategoryId))
                return new EBMCreateProductResult { Success = false, ErrorMessage = "EBM Category ID not configured. Set it in EBM Configuration." };

            var payload = new EBMCreateProductPayload
            {
                ProductName = productName,
                CategoryId = settings.EBMCategoryId,
                BusinessId = settings.EBMBusinessId,
                BranchId = settings.EBMBranchId,
                Variants = new List<EBMProductVariant>
                {
                    new() { VariantName = productName, RetailPrice = retailPrice, SupplyPrice = supplyPrice }
                }
            };

            var client = _httpClientFactory.CreateClient("EBM");

            // YegoBox uses snake_case for product creation
            var snakeCaseOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            var json = JsonSerializer.Serialize(payload, snakeCaseOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{settings.EBMServerUrl.TrimEnd('/')}/products";
            var response = await client.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = ExtractProductIds(responseBody);
                _logger.LogInformation("EBM product created for org {OrgId}: product={ProductId}, variant={VariantId}, stock={StockId}",
                    orgId, result.ProductId, result.VariantId, result.StockId);
                return result;
            }

            _logger.LogWarning("EBM product creation failed for org {OrgId}: {StatusCode} - {Body}",
                orgId, response.StatusCode, responseBody);
            return new EBMCreateProductResult
            {
                Success = false,
                ErrorMessage = $"EBM API returned {(int)response.StatusCode}: {responseBody}",
                RawResponse = responseBody
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EBM product creation exception for org {OrgId}", orgId);
            return new EBMCreateProductResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<bool> DeleteProductAsync(Guid orgId, string productId)
    {
        try
        {
            var settings = await GetSettings(orgId);
            if (settings == null || !settings.EBMEnabled || string.IsNullOrEmpty(settings.EBMServerUrl))
                return false;

            var client = _httpClientFactory.CreateClient("EBM");
            var response = await client.DeleteAsync($"{settings.EBMServerUrl.TrimEnd('/')}/products/{productId}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("EBM product {ProductId} deleted for org {OrgId}", productId, orgId);
                return true;
            }

            var body = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("EBM product deletion failed for org {OrgId}: {StatusCode} - {Body}",
                orgId, response.StatusCode, body);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EBM product deletion exception for org {OrgId}", orgId);
            return false;
        }
    }

    private static EBMCreateProductResult ExtractProductIds(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            // Try to get from "data" wrapper or directly
            var data = root.TryGetProperty("data", out var d) ? d : root;

            string? productId = null;
            string? variantId = null;
            string? stockId = null;

            // Product ID
            if (data.TryGetProperty("product_id", out var pid))
                productId = pid.GetString();
            else if (data.TryGetProperty("id", out var id))
                productId = id.GetString();

            // Variant IDs — array or single
            if (data.TryGetProperty("variant_ids", out var vids) && vids.ValueKind == JsonValueKind.Array && vids.GetArrayLength() > 0)
                variantId = vids[0].GetString();
            else if (data.TryGetProperty("variants", out var variants) && variants.ValueKind == JsonValueKind.Array && variants.GetArrayLength() > 0)
            {
                var v = variants[0];
                if (v.TryGetProperty("id", out var vid)) variantId = vid.GetString();
                if (v.TryGetProperty("stock_id", out var sid)) stockId = sid.GetString();
            }

            // Stock IDs — array or single
            if (stockId == null && data.TryGetProperty("stock_ids", out var sids) && sids.ValueKind == JsonValueKind.Array && sids.GetArrayLength() > 0)
                stockId = sids[0].GetString();

            return new EBMCreateProductResult
            {
                Success = true,
                ProductId = productId,
                VariantId = variantId,
                StockId = stockId,
                RawResponse = responseBody
            };
        }
        catch
        {
            return new EBMCreateProductResult { Success = true, RawResponse = responseBody };
        }
    }

    private async Task<Domain.Entities.OrganizationSettings?> GetSettings(Guid orgId)
    {
        return await _unitOfWork.OrganizationSettings.Query()
            .FirstOrDefaultAsync(s => s.OrganizationId == orgId);
    }

    private static bool IsInvoiceDuplicateError(string responseBody)
    {
        if (string.IsNullOrEmpty(responseBody)) return false;
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("resultCd", out var code))
            {
                var codeStr = code.GetString();
                if (codeStr == "924") return true; // "Invoice number already exists"
            }
            if (doc.RootElement.TryGetProperty("resultMsg", out var msg))
            {
                var msgStr = msg.GetString();
                if (msgStr != null && msgStr.Contains("Invoice number already exists", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        catch { }
        return false;
    }

    private static string? TryExtractReceiptCode(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);

            // YegoBox returns {"previewUrl":"https://.../receipts/preview/<receipt-id>"}
            // Return the full URL so cashiers can view/print the EBM receipt
            if (doc.RootElement.TryGetProperty("previewUrl", out var previewUrl))
            {
                var url = previewUrl.GetString();
                if (!string.IsNullOrEmpty(url))
                    return url;
            }

            // Fallback: try other common response formats
            if (doc.RootElement.TryGetProperty("receiptCode", out var code))
                return code.GetString();
            if (doc.RootElement.TryGetProperty("receipt_code", out var code2))
                return code2.GetString();
            if (doc.RootElement.TryGetProperty("data", out var data))
            {
                if (data.TryGetProperty("receiptCode", out var nestedCode))
                    return nestedCode.GetString();
                if (data.TryGetProperty("rcpt_no", out var rcptNo))
                    return rcptNo.ToString();
            }
        }
        catch { }
        return null;
    }
}
