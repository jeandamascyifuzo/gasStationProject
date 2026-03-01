namespace Escale.API.Domain.Entities;

public class OrganizationSettings : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string CompanyName { get; set; } = string.Empty;
    public decimal TaxRate { get; set; } = 0.18m;
    public string Currency { get; set; } = "RWF";
    public string? ReceiptHeader { get; set; }
    public string? ReceiptFooter { get; set; }
    public bool EBMEnabled { get; set; }
    public string? EBMServerUrl { get; set; }
    public string? EBMBusinessId { get; set; }
    public string? EBMBranchId { get; set; }
    public string? EBMCompanyName { get; set; }
    public string? EBMCompanyAddress { get; set; }
    public string? EBMCompanyPhone { get; set; }
    public string? EBMCompanyTIN { get; set; }
    public string? EBMCategoryId { get; set; }
    public bool AutoPrintReceipt { get; set; }
    public bool RequireCustomerInfo { get; set; }
    public decimal MinimumSaleAmount { get; set; } = 1000m;
    public decimal MaximumSaleAmount { get; set; } = 10_000_000m;
    public bool AllowNegativeStock { get; set; }
    public decimal LowStockThreshold { get; set; } = 0.20m;
    public decimal CriticalStockThreshold { get; set; } = 0.10m;
}
