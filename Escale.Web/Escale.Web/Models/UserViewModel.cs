using System.ComponentModel.DataAnnotations;

namespace Escale.Web.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Phone]
        public string? Phone { get; set; }
        
        [Required]
        public string Role { get; set; } = string.Empty;
        
        public int? StationId { get; set; }
        public string? StationName { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime? LastLogin { get; set; }
        
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }

    public class UserViewModel
    {
        public List<User> Users { get; set; } = new();
        public List<Station> Stations { get; set; } = new();
        public List<string> Roles { get; set; } = new() { "Admin", "Manager", "Cashier" };
    }
}
