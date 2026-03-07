using System.ComponentModel.DataAnnotations;

namespace Escale.Web.Models
{
    public class FuelType
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal PricePerLiter { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public string? EBMProductId { get; set; }

        public string? EBMVariantId { get; set; }

        public decimal? EBMSupplyPrice { get; set; }

        public bool IsEBMRegistered => !string.IsNullOrEmpty(EBMProductId) || !string.IsNullOrEmpty(EBMVariantId) || EBMSupplyPrice.HasValue;

        public DateTime CreatedAt { get; set; }

        public string Status => IsDeleted ? "Deleted" : IsActive ? "Active" : "Inactive";
    }
}
