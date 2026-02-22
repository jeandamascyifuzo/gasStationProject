using Escale.API.DTOs.Dashboard;

namespace Escale.API.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(Guid? stationId = null, DateTime? date = null);
}
