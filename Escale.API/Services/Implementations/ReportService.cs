using System.Text;
using Escale.API.Data.Repositories;
using Escale.API.DTOs.Reports;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ReportService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<SalesReportDto> GetSalesReportAsync(DateTime startDate, DateTime endDate, Guid? stationId = null)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var query = _unitOfWork.Transactions.Query()
            .Include(t => t.FuelType)
            .Where(t => t.OrganizationId == orgId && t.TransactionDate >= startDate && t.TransactionDate <= endDate);

        if (stationId.HasValue)
            query = query.Where(t => t.StationId == stationId.Value);

        var transactions = await query.ToListAsync();

        var salesByFuel = transactions
            .GroupBy(t => t.FuelType.Name)
            .Select(g => new SalesByFuelTypeDto
            {
                FuelType = g.Key,
                Liters = g.Sum(t => t.Liters),
                Amount = g.Sum(t => t.Total),
                TransactionCount = g.Count()
            }).ToList();

        var salesByPayment = transactions
            .GroupBy(t => t.PaymentMethod.ToString())
            .Select(g => new SalesByPaymentMethodDto
            {
                PaymentMethod = g.Key,
                Amount = g.Sum(t => t.Total),
                TransactionCount = g.Count()
            }).ToList();

        var dailySales = transactions
            .GroupBy(t => t.TransactionDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailySalesDto
            {
                Date = g.Key,
                Amount = g.Sum(t => t.Total),
                TransactionCount = g.Count()
            }).ToList();

        return new SalesReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalSales = transactions.Sum(t => t.Total),
            TransactionCount = transactions.Count,
            SalesByFuel = salesByFuel,
            SalesByPayment = salesByPayment,
            DailySales = dailySales
        };
    }

    public async Task<List<InventoryReportDto>> GetInventoryReportAsync(Guid? stationId = null)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var query = _unitOfWork.InventoryItems.Query()
            .Include(i => i.Station)
            .Include(i => i.FuelType)
            .Include(i => i.RefillRecords)
            .Where(i => i.OrganizationId == orgId);

        if (stationId.HasValue)
            query = query.Where(i => i.StationId == stationId.Value);

        var items = await query.ToListAsync();

        return items.Select(i =>
        {
            var pct = i.Capacity > 0 ? Math.Round(i.CurrentLevel / i.Capacity * 100, 1) : 0;
            return new InventoryReportDto
            {
                StationName = i.Station.Name,
                FuelType = i.FuelType.Name,
                CurrentLevel = i.CurrentLevel,
                Capacity = i.Capacity,
                PercentageFull = pct,
                Status = pct < 10 ? "Critical" : pct < 25 ? "Low Stock" : "Normal",
                RefillCount = i.RefillRecords.Count,
                TotalRefillCost = i.RefillRecords.Sum(r => r.TotalCost)
            };
        }).ToList();
    }

    public async Task<List<EmployeeReportDto>> GetEmployeeReportAsync(DateTime startDate, DateTime endDate)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var users = await _unitOfWork.Users.Query()
            .Include(u => u.ProcessedTransactions.Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate))
            .Include(u => u.Shifts.Where(s => s.StartTime >= startDate && s.StartTime <= endDate))
            .Where(u => u.OrganizationId == orgId)
            .ToListAsync();

        return users.Select(u => new EmployeeReportDto
        {
            UserId = u.Id,
            FullName = u.FullName,
            Role = u.Role.ToString(),
            TransactionCount = u.ProcessedTransactions.Count,
            TotalSales = u.ProcessedTransactions.Sum(t => t.Total),
            ShiftCount = u.Shifts.Count,
            TotalHoursWorked = u.Shifts.Sum(s => ((s.EndTime ?? DateTime.UtcNow) - s.StartTime).TotalHours)
        }).ToList();
    }

    public async Task<List<CustomerReportDto>> GetCustomerReportAsync(DateTime startDate, DateTime endDate)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var customers = await _unitOfWork.Customers.Query()
            .Include(c => c.Transactions.Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate))
            .Where(c => c.OrganizationId == orgId)
            .ToListAsync();

        return customers.Select(c => new CustomerReportDto
        {
            CustomerId = c.Id,
            Name = c.Name,
            Type = c.Type.ToString(),
            TransactionCount = c.Transactions.Count,
            TotalSpent = c.Transactions.Sum(t => t.Total),
            TotalLiters = c.Transactions.Sum(t => t.Liters),
            CurrentCredit = c.CurrentCredit
        }).ToList();
    }

    public async Task<FinancialReportDto> GetFinancialReportAsync(DateTime startDate, DateTime endDate)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var transactions = await _unitOfWork.Transactions.Query()
            .Where(t => t.OrganizationId == orgId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .ToListAsync();

        var refillCost = await _unitOfWork.RefillRecords.Query()
            .Where(r => r.InventoryItem.OrganizationId == orgId && r.RefillDate >= startDate && r.RefillDate <= endDate)
            .SumAsync(r => (decimal?)r.TotalCost) ?? 0;

        var totalRevenue = transactions.Sum(t => t.Total);
        var totalVAT = transactions.Sum(t => t.VATAmount);

        var dailyRevenue = transactions
            .GroupBy(t => t.TransactionDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailySalesDto
            {
                Date = g.Key,
                Amount = g.Sum(t => t.Total),
                TransactionCount = g.Count()
            }).ToList();

        return new FinancialReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = totalRevenue,
            TotalVAT = totalVAT,
            TotalRefillCost = refillCost,
            GrossProfit = totalRevenue - totalVAT - refillCost,
            DailyRevenue = dailyRevenue
        };
    }

    public async Task<byte[]> ExportTransactionsAsync(DateTime startDate, DateTime endDate, string format = "csv")
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var transactions = await _unitOfWork.Transactions.Query()
            .Include(t => t.FuelType)
            .Include(t => t.Station)
            .Include(t => t.Cashier)
            .Where(t => t.OrganizationId == orgId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("ReceiptNumber,Date,Station,FuelType,Liters,PricePerLiter,Subtotal,VAT,Total,PaymentMethod,Cashier,Customer");

        foreach (var t in transactions)
        {
            sb.AppendLine($"{t.ReceiptNumber},{t.TransactionDate:yyyy-MM-dd HH:mm},{t.Station.Name},{t.FuelType.Name},{t.Liters},{t.PricePerLiter},{t.Subtotal},{t.VATAmount},{t.Total},{t.PaymentMethod},{t.Cashier.FullName},{t.CustomerName ?? "Walk-in"}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
