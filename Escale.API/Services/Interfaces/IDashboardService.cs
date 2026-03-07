using Escale.API.DTOs.Dashboard;

namespace Escale.API.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(Guid? stationId = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<StationPerformanceDto>> GetStationPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null, int top = 5);
}
