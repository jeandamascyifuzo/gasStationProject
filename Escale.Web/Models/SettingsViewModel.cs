using System.ComponentModel.DataAnnotations;

namespace Escale.Web.Models
{
    public class SettingsViewModel
    {
        public string CompanyName { get; set; } = string.Empty;
        public decimal TaxRate { get; set; }
        public string Currency { get; set; } = "RWF";
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

        // EBM Status
        public bool EBMConnected { get; set; }
        public DateTime? EBMLastSync { get; set; }
        public string EBMStatus { get; set; } = string.Empty;

        // Fuel prices from FuelTypes API
        public List<FuelType> FuelTypes { get; set; } = new();
    }
}
