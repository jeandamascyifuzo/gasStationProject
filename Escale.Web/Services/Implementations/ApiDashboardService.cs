using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;

namespace Escale.Web.Services.Implementations;

public class ApiDashboardService : BaseApiService, IApiDashboardService
{
    public ApiDashboardService(HttpClient httpClient) : base(httpClient) { }

    public async Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync(Guid? stationId = null, DateTime? date = null)
    {
        var query = new List<string>();
        if (stationId.HasValue) query.Add($"stationId={stationId}");
        if (date.HasValue) query.Add($"date={date.Value:yyyy-MM-dd}");
        var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";
        return await GetAsync<DashboardSummaryDto>($"/api/dashboard/summary{qs}");
    }

    public async Task<ApiResponse<List<StationPerformanceDto>>> GetStationPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null, int top = 5)
    {
        var query = new List<string>();
        if (startDate.HasValue) query.Add($"startDate={startDate.Value:yyyy-MM-dd}");
        if (endDate.HasValue) query.Add($"endDate={endDate.Value:yyyy-MM-dd}");
        query.Add($"top={top}");
        var qs = "?" + string.Join("&", query);
        return await GetAsync<List<StationPerformanceDto>>($"/api/dashboard/station-performance{qs}");
    }
}
