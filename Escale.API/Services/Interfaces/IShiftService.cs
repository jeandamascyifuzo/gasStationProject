using Escale.API.DTOs.Shifts;

namespace Escale.API.Services.Interfaces;

public interface IShiftService
{
    Task<ShiftResponseDto?> GetCurrentShiftAsync(Guid userId, Guid stationId);
    Task<ClockResponseDto> ClockAsync(ClockRequestDto request);
    Task<ShiftSummaryDto?> GetShiftSummaryAsync(Guid userId, Guid stationId);
}
