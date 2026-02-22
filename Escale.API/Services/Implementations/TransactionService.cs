using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.DTOs.Common;
using Escale.API.DTOs.Transactions;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public TransactionService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<PagedResult<TransactionResponseDto>> GetTransactionsAsync(TransactionFilterDto filter)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var query = _unitOfWork.Transactions.Query()
            .Include(t => t.FuelType)
            .Include(t => t.Cashier)
            .Include(t => t.Station)
            .Where(t => t.OrganizationId == orgId);

        if (filter.StationId.HasValue)
            query = query.Where(t => t.StationId == filter.StationId.Value);
        if (filter.StartDate.HasValue)
            query = query.Where(t => t.TransactionDate >= filter.StartDate.Value);
        if (filter.EndDate.HasValue)
            query = query.Where(t => t.TransactionDate <= filter.EndDate.Value);
        if (filter.FuelTypeId.HasValue)
            query = query.Where(t => t.FuelTypeId == filter.FuelTypeId.Value);

        var totalCount = await query.CountAsync();
        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<TransactionResponseDto>
        {
            Items = _mapper.Map<List<TransactionResponseDto>>(transactions),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<TransactionResponseDto> GetTransactionByIdAsync(Guid id)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var transaction = await _unitOfWork.Transactions.Query()
            .Include(t => t.FuelType)
            .Include(t => t.Cashier)
            .Include(t => t.Station)
            .FirstOrDefaultAsync(t => t.Id == id && t.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("Transaction not found");
        return _mapper.Map<TransactionResponseDto>(transaction);
    }
}
