namespace Escale.API.DTOs.EBM;

public class EBMSellPayload
{
    public List<EBMSellItem> Items { get; set; } = new();
    public string? CustomerName { get; set; }
    public string? ClientId { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyTin { get; set; }
    public string? CustomerPhone { get; set; }
}

public class EBMSellItem
{
    public string VariantId { get; set; } = string.Empty;
    public decimal Qty { get; set; }
}

public class EBMSellResult
{
    public bool Success { get; set; }
    public string? ReceiptCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RawResponse { get; set; }
}

public class EBMUpdatePricePayload
{
    public string VariantId { get; set; } = string.Empty;
    public decimal RetailPrice { get; set; }
    public decimal SupplyPrice { get; set; }
}

public class EBMUpdateStockPayload
{
    public string StockId { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
}

public class EBMCreateProductPayload
{
    public string ProductName { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string BusinessId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public List<EBMProductVariant> Variants { get; set; } = new();
}

public class EBMProductVariant
{
    public string VariantName { get; set; } = string.Empty;
    public decimal RetailPrice { get; set; }
    public decimal SupplyPrice { get; set; }
}

public class EBMCreateProductResult
{
    public bool Success { get; set; }
    public string? ProductId { get; set; }
    public string? VariantId { get; set; }
    public string? StockId { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RawResponse { get; set; }
}
