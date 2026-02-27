using System.ComponentModel.DataAnnotations;

namespace Escale.Web.Models
{
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty;

        public List<StationInfo> AssignedStations { get; set; } = new();

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public List<Guid> StationIds { get; set; } = new();
    }

    public class StationInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    public class UserViewModel
    {
        public List<User> Users { get; set; } = new();
        public List<Station> Stations { get; set; } = new();
        public List<string> Roles { get; set; } = new() { "Admin", "Manager", "Cashier" };
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
    }
}
