namespace Escale.Web.Models;

public class OrganizationListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? TIN { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int StationCount { get; set; }
    public int UserCount { get; set; }
    public string Status => IsActive ? "Active" : "Inactive";
}

public class OrganizationViewModel
{
    public List<OrganizationListItem> Organizations { get; set; } = new();
}

public class OrganizationDetailsViewModel
{
    public OrganizationListItem Organization { get; set; } = new();
    public List<Station> Stations { get; set; } = new();
}
