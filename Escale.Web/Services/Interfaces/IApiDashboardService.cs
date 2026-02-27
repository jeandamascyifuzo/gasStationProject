using Escale.Web.Models.Api;

namespace Escale.Web.Services.Interfaces;

public interface IApiDashboardService
{
    Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync(Guid? stationId = null, DateTime? date = null);
}
