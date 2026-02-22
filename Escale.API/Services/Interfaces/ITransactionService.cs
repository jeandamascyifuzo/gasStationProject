using Escale.API.DTOs.Common;
using Escale.API.DTOs.Transactions;

namespace Escale.API.Services.Interfaces;

public interface ITransactionService
{
    Task<PagedResult<TransactionResponseDto>> GetTransactionsAsync(TransactionFilterDto filter);
    Task<TransactionResponseDto> GetTransactionByIdAsync(Guid id);
}
