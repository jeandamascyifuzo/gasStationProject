namespace Escale.Web.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public decimal Liters { get; set; }
        public decimal PricePerLiter { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VAT { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? EBMCode { get; set; }
        public bool EBMSent { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; } = "Completed";
    }

    public class TransactionFilterViewModel
    {
        public Guid? StationId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? FuelTypeId { get; set; }
        public string? PaymentMethod { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public List<Transaction> Transactions { get; set; } = new();
        public List<Station> Stations { get; set; } = new();
        public List<FuelType> FuelTypes { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
