using System.ComponentModel.DataAnnotations;

namespace Escale.Web.Models
{
    public class FuelType
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Code { get; set; } = string.Empty;
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal SellingPrice { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal CostPrice { get; set; }
        
        [Required]
        [Range(0, 100)]
        public decimal VATRate { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
