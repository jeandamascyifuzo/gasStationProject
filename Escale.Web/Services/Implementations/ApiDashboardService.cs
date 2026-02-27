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
}
