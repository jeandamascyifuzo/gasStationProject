namespace Escale.API.DTOs.Stations;

public class StationResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public string? Manager { get; set; }
    public Guid? ManagerId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateStationRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? ManagerId { get; set; }
}

public class UpdateStationRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public Guid? ManagerId { get; set; }
}

public class StationDetailResponseDto : StationResponseDto
{
    public int EmployeeCount { get; set; }
    public int TodayTransactionCount { get; set; }
    public decimal TodaySales { get; set; }
}
