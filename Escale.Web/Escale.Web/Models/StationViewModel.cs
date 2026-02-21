using System.ComponentModel.DataAnnotations;

namespace Escale.Web.Models
{
    public class Station
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Location { get; set; } = string.Empty;
        
        public string Address { get; set; } = string.Empty;
        
        [Phone]
        public string ContactNumber { get; set; } = string.Empty;
        
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        
        public string Status { get; set; } = "Active";
        
        public int PumpCount { get; set; }
    }

    public class StationViewModel
    {
        public List<Station> Stations { get; set; } = new();
        public List<User> Managers { get; set; } = new();
    }
}
