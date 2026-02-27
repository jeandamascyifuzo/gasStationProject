namespace Escale.API.DTOs.Organizations;

public class OrganizationResponseDto
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
}

public class CreateOrganizationRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? TIN { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class UpdateOrganizationRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? TIN { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
}
