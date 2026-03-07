using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Enums;
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

    public async Task<List<StationPerformanceDto>> GetStationPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null, int top = 5)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var start = startDate?.Date ?? DateTime.UtcNow.Date;
        var end = endDate?.Date ?? DateTime.UtcNow.Date;

        var transactions = await _unitOfWork.Transactions.Query()
            .AsNoTracking()
            .Include(t => t.Station)
            .Where(t => t.OrganizationId == orgId
                && t.TransactionDate >= start
                && t.TransactionDate < end.AddDays(1))
            .Select(t => new
            {
                t.StationId,
                StationName = t.Station.Name,
                t.Total,
                t.Liters,
                t.PaymentMethod
            })
            .ToListAsync();

        var stationPerformance = transactions
            .GroupBy(t => new { t.StationId, t.StationName })
            .Select(g => new StationPerformanceDto
            {
                StationId = g.Key.StationId,
                StationName = g.Key.StationName,
                TotalSales = g.Sum(t => t.Total),
                TransactionCount = g.Count(),
                TotalLiters = g.Sum(t => t.Liters),
                CreditSales = g.Where(t => t.PaymentMethod == PaymentMethod.Credit).Sum(t => t.Total),
                CashSales = g.Where(t => t.PaymentMethod != PaymentMethod.Credit).Sum(t => t.Total)
            })
            .OrderByDescending(s => s.TotalSales)
            .Take(top)
            .ToList();

        for (int i = 0; i < stationPerformance.Count; i++)
            stationPerformance[i].Rank = i + 1;

        return stationPerformance;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(Guid? stationId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var start = startDate?.Date ?? DateTime.UtcNow.Date;
        var end = (endDate?.Date ?? start).AddDays(1);

        var txQuery = _unitOfWork.Transactions.Query()
            .AsNoTracking()
            .Where(t => t.OrganizationId == orgId && t.TransactionDate >= start && t.TransactionDate < end);

        if (stationId.HasValue)
            txQuery = txQuery.Where(t => t.StationId == stationId.Value);

        // Total aggregation
        var aggregate = await txQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalSales = g.Sum(t => (decimal?)t.Total) ?? 0,
                Count = g.Count()
            })
            .FirstOrDefaultAsync();

        // Credit aggregation (separate query for EF Core compatibility)
        var creditAggregate = await txQuery
            .Where(t => t.PaymentMethod == PaymentMethod.Credit)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                CreditSales = g.Sum(t => (decimal?)t.Total) ?? 0,
                CreditCount = g.Count()
            })
            .FirstOrDefaultAsync();

        var todaysSales = aggregate?.TotalSales ?? 0;
        var transactionCount = aggregate?.Count ?? 0;
        var averageSale = transactionCount > 0 ? todaysSales / transactionCount : 0;

        // Low stock alerts
        var inventoryQuery = _unitOfWork.InventoryItems.Query()
            .AsNoTracking()
            .Include(i => i.FuelType)
            .Where(i => i.OrganizationId == orgId && i.Capacity > 0);

        if (stationId.HasValue)
            inventoryQuery = inventoryQuery.Where(i => i.StationId == stationId.Value);

        var lowStockAlerts = await inventoryQuery
            .Where(i => i.CurrentLevel / i.Capacity < 0.25m)
            .Select(i => new StockAlertDto
            {
                FuelType = i.FuelType.Name,
                CurrentLevel = i.CurrentLevel,
                Capacity = i.Capacity,
                PercentageFull = Math.Round(i.CurrentLevel / i.Capacity * 100, 1)
            })
            .ToListAsync();

        // Recent transactions (filtered by selected date range)
        var recentQuery = _unitOfWork.Transactions.Query()
            .AsNoTracking()
            .Include(t => t.FuelType)
            .Include(t => t.Customer)
            .Include(t => t.Station)
            .Include(t => t.Cashier)
            .Where(t => t.OrganizationId == orgId && t.TransactionDate >= start && t.TransactionDate < end);

        if (stationId.HasValue)
            recentQuery = recentQuery.Where(t => t.StationId == stationId.Value);

        var recentTransactions = await recentQuery
            .OrderByDescending(t => t.TransactionDate)
            .Take(10)
            .ToListAsync();

        return new DashboardSummaryDto
        {
            TodaysSales = todaysSales,
            TransactionCount = transactionCount,
            AverageSale = Math.Round(averageSale, 2),
            CreditSales = creditAggregate?.CreditSales ?? 0,
            CreditTransactionCount = creditAggregate?.CreditCount ?? 0,
            LowStockAlerts = lowStockAlerts,
            RecentTransactions = _mapper.Map<List<RecentTransactionDto>>(recentTransactions)
        };
    }
}
