using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;

namespace Escale.Web.Services.Implementations;

public class ApiTransactionService : BaseApiService, IApiTransactionService
{
    public ApiTransactionService(HttpClient httpClient) : base(httpClient) { }

    public async Task<ApiResponse<PagedResult<TransactionResponseDto>>> GetAllAsync(TransactionFilterDto filter)
    {
        var query = new List<string>
        {
            $"page={filter.Page}",
            $"pageSize={filter.PageSize}"
        };
        if (filter.StationId.HasValue) query.Add($"stationId={filter.StationId}");
        if (filter.StartDate.HasValue) query.Add($"startDate={filter.StartDate.Value:yyyy-MM-dd}");
        if (filter.EndDate.HasValue) query.Add($"endDate={filter.EndDate.Value:yyyy-MM-dd}");
        if (filter.FuelTypeId.HasValue) query.Add($"fuelTypeId={filter.FuelTypeId}");
        if (!string.IsNullOrEmpty(filter.PaymentMethod)) query.Add($"paymentMethod={Uri.EscapeDataString(filter.PaymentMethod)}");

        return await GetAsync<PagedResult<TransactionResponseDto>>($"/api/transactions?{string.Join("&", query)}");
    }

    public async Task<ApiResponse<TransactionResponseDto>> GetByIdAsync(Guid id)
        => await GetAsync<TransactionResponseDto>($"/api/transactions/{id}");
}
