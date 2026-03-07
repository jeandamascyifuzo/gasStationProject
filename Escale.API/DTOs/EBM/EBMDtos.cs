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
    public string? CustomerTin { get; set; }
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
    public string LastTouched { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
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
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = "SKU001";
    public string Barcode { get; set; } = "BARCODE001";
    public decimal RetailPrice { get; set; }
    public decimal SupplyPrice { get; set; }
    public decimal Quantity { get; set; }
    public string Color { get; set; } = "N/A";
    public string PackagingUnit { get; set; } = "PCS";
    public string ItemCd { get; set; } = "ITEM001";
    public string IsrcAplcbYn { get; set; } = "Y";
    public string UseYn { get; set; } = "Y";
    public string TaxTyCd { get; set; } = "B";
    public string BhfId { get; set; } = "BHF001";
    public string Bcd { get; set; } = "BCD001";
    public decimal TaxPercentage { get; set; } = 18.0m;
    public string ItemClsCd { get; set; } = "5020230602";
    public string IsrccNm { get; set; } = "N/A";
    public decimal IsrcRt { get; set; }
    public string QtyUnitCd { get; set; } = "U";
    public string PkgUnitCd { get; set; } = "CT";
    public int Pkg { get; set; } = 1;
    public string ItemTyCd { get; set; } = "2";
    public string ModrNm { get; set; } = "System";
    public string ModrId { get; set; } = "System";
    public decimal Prc { get; set; }
    public decimal DftPrc { get; set; }
    public int ItemSeq { get; set; } = 1;
    public string ItemStdNm { get; set; } = string.Empty;
    public string SpplrItemNm { get; set; } = string.Empty;
    public bool EbmSynced { get; set; } = true;
    public string OrgnNatCd { get; set; } = "RW";
    public string ItemNm { get; set; } = string.Empty;
    public string RegrNm { get; set; } = "System";
    public decimal SplyAmt { get; set; }
    public long Tin { get; set; }
    public decimal DcRt { get; set; }
    public string RegrId { get; set; } = "System";
    public string SpplrNm { get; set; } = "N/A";
    public string AgntNm { get; set; } = "N/A";
    public string AddInfo { get; set; } = "N/A";
    public string LastTouched { get; set; } = "2026-01-01T00:00:00Z";
    public string ExptNatCd { get; set; } = "RW";
    public string DclNo { get; set; } = "N/A";
    public string TaskCd { get; set; } = "N/A";
    public string DclDe { get; set; } = "2026-01-01";
    public string ImptItemSttsCd { get; set; } = "ACTIVE";
    public decimal RsdQty { get; set; }
    public string IsrccCd { get; set; } = "N/A";
    public decimal IsrcAmt { get; set; }
    public decimal DcAmt { get; set; }
    public string ExpirationDate { get; set; } = "2030-12-31";
    public string Unit { get; set; } = "LTR";
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = "Fuel";
    public string BarCode { get; set; } = "BARCODE001";
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
