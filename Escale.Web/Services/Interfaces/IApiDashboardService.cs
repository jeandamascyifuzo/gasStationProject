using Escale.Web.Models.Api;

namespace Escale.Web.Services.Interfaces;

public interface IApiDashboardService
{
    Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync(Guid? stationId = null, DateTime? date = null);
    Task<ApiResponse<List<StationPerformanceDto>>> GetStationPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null, int top = 5);
}
