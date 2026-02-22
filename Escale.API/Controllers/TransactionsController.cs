using Escale.API.DTOs.Common;
using Escale.API.DTOs.Transactions;
using Escale.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TransactionResponseDto>>>> GetTransactions([FromQuery] TransactionFilterDto filter)
    {
        var result = await _transactionService.GetTransactionsAsync(filter);
        return Ok(ApiResponse<PagedResult<TransactionResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TransactionResponseDto>>> GetTransaction(Guid id)
    {
        var result = await _transactionService.GetTransactionByIdAsync(id);
        return Ok(ApiResponse<TransactionResponseDto>.SuccessResponse(result));
    }
}
