using Escale.Web.Models.Api;

namespace Escale.Web.Services.Interfaces;

public interface IApiTransactionService
{
    Task<ApiResponse<PagedResult<TransactionResponseDto>>> GetAllAsync(TransactionFilterDto filter);
    Task<ApiResponse<TransactionResponseDto>> GetByIdAsync(Guid id);
}
