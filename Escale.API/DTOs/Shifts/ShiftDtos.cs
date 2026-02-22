namespace Escale.API.DTOs.Shifts;

public class ShiftResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid StationId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsActive { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalSales { get; set; }
}

public class ClockRequestDto
{
    public Guid UserId { get; set; }
    public Guid StationId { get; set; }
    public bool IsClockIn { get; set; }
}

public class ClockResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ShiftResponseDto? Shift { get; set; }
}

public class ShiftSummaryDto
{
    public Guid ShiftId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Duration { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalSales { get; set; }
    public decimal CashSales { get; set; }
    public decimal MobileMoneySales { get; set; }
    public decimal CardSales { get; set; }
    public decimal CreditSales { get; set; }
}
