using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.DTOs.Dashboard;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public DashboardService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(Guid? stationId = null, DateTime? date = null)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var targetDate = date?.Date ?? DateTime.UtcNow.Date;

        var txQuery = _unitOfWork.Transactions.Query()
            .Where(t => t.OrganizationId == orgId && t.TransactionDate.Date == targetDate);

        if (stationId.HasValue)
            txQuery = txQuery.Where(t => t.StationId == stationId.Value);

        var todaysSales = await txQuery.SumAsync(t => (decimal?)t.Total) ?? 0;
        var transactionCount = await txQuery.CountAsync();
        var averageSale = transactionCount > 0 ? todaysSales / transactionCount : 0;

        // Low stock alerts
        var inventoryQuery = _unitOfWork.InventoryItems.Query()
            .Include(i => i.FuelType)
            .Where(i => i.OrganizationId == orgId && i.Capacity > 0);

        if (stationId.HasValue)
            inventoryQuery = inventoryQuery.Where(i => i.StationId == stationId.Value);

        var inventoryItems = await inventoryQuery.ToListAsync();
        var lowStockAlerts = inventoryItems
            .Where(i => i.CurrentLevel / i.Capacity < 0.25m)
            .Select(i => new StockAlertDto
            {
                FuelType = i.FuelType.Name,
                CurrentLevel = i.CurrentLevel,
                Capacity = i.Capacity,
                PercentageFull = Math.Round(i.CurrentLevel / i.Capacity * 100, 1)
            })
            .ToList();

        // Recent transactions
        var recentQuery = _unitOfWork.Transactions.Query()
            .Include(t => t.FuelType)
            .Where(t => t.OrganizationId == orgId);

        if (stationId.HasValue)
            recentQuery = recentQuery.Where(t => t.StationId == stationId.Value);

        var recentTransactions = await recentQuery
            .OrderByDescending(t => t.TransactionDate)
            .Take(5)
            .ToListAsync();

        return new DashboardSummaryDto
        {
            TodaysSales = todaysSales,
            TransactionCount = transactionCount,
            AverageSale = Math.Round(averageSale, 2),
            LowStockAlerts = lowStockAlerts,
            RecentTransactions = _mapper.Map<List<RecentTransactionDto>>(recentTransactions)
        };
    }
}
