using System.ComponentModel.DataAnnotations;

namespace Escale.Web.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Station { get; set; } = string.Empty;
        public string Pump { get; set; } = string.Empty;
        public string FuelType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? EBMCode { get; set; }
        public DateTime TransactionTime { get; set; }
        public string Status { get; set; } = "Completed";
    }

    public class TransactionFilterViewModel
    {
        public int? StationId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? FuelTypeId { get; set; }
        public string? SearchTerm { get; set; }
        public List<Transaction> Transactions { get; set; } = new();
        public List<Station> Stations { get; set; } = new();
        public List<FuelType> FuelTypes { get; set; } = new();
    }
}
