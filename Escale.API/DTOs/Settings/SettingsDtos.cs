namespace Escale.API.DTOs.Settings;

public class AppSettingsResponseDto
{
    public string CompanyName { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? ReceiptHeader { get; set; }
    public string? ReceiptFooter { get; set; }
    public bool EBMEnabled { get; set; }
    public string? EBMServerUrl { get; set; }
    public bool AutoPrintReceipt { get; set; }
    public bool RequireCustomerInfo { get; set; }
    public decimal MinimumSaleAmount { get; set; }
    public decimal MaximumSaleAmount { get; set; }
    public bool AllowNegativeStock { get; set; }
    public decimal LowStockThreshold { get; set; }
    public decimal CriticalStockThreshold { get; set; }
}

public class UpdateSettingsRequestDto
{
    public string CompanyName { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? ReceiptHeader { get; set; }
    public string? ReceiptFooter { get; set; }
    public bool EBMEnabled { get; set; }
    public string? EBMServerUrl { get; set; }
    public bool AutoPrintReceipt { get; set; }
    public bool RequireCustomerInfo { get; set; }
    public decimal MinimumSaleAmount { get; set; }
    public decimal MaximumSaleAmount { get; set; }
    public bool AllowNegativeStock { get; set; }
    public decimal LowStockThreshold { get; set; }
    public decimal CriticalStockThreshold { get; set; }
}

public class EbmConfigRequestDto
{
    public bool EBMEnabled { get; set; }
    public string? EBMServerUrl { get; set; }
    public string? EBMBusinessId { get; set; }
    public string? EBMBranchId { get; set; }
    public string? EBMCompanyName { get; set; }
    public string? EBMCompanyAddress { get; set; }
    public string? EBMCompanyPhone { get; set; }
    public string? EBMCompanyTIN { get; set; }
    public string? EBMCategoryId { get; set; }
}

public class EbmConfigResponseDto
{
    public bool EBMEnabled { get; set; }
    public string? EBMServerUrl { get; set; }
    public string? EBMBusinessId { get; set; }
    public string? EBMBranchId { get; set; }
    public string? EBMCompanyName { get; set; }
    public string? EBMCompanyAddress { get; set; }
    public string? EBMCompanyPhone { get; set; }
    public string? EBMCompanyTIN { get; set; }
    public string? EBMCategoryId { get; set; }
    public bool IsConfigured { get; set; }
}

public class EbmStatusDto
{
    public bool IsConnected { get; set; }
    public string ServerUrl { get; set; } = string.Empty;
    public DateTime? LastSyncAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsConfigured { get; set; }
}
