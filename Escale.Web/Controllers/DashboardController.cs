using Escale.Web.Models;
using Escale.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IApiDashboardService _dashboardService;
        private readonly IApiStationService _stationService;
        private readonly IApiReportService _reportService;

        public DashboardController(
            IApiDashboardService dashboardService,
            IApiStationService stationService,
            IApiReportService reportService)
        {
            _dashboardService = dashboardService;
            _stationService = stationService;
            _reportService = reportService;
        }

        public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null)
        {
            // SuperAdmin is platform-level, redirect to Organizations
            if (HttpContext.Session.GetString("UserRole") == "SuperAdmin")
                return RedirectToAction("Index", "Organizations");

            var start = startDate ?? DateTime.Today;
            var end = endDate ?? DateTime.Today;

            // Ensure start <= end
            if (start > end) (start, end) = (end, start);

            ViewBag.StartDate = start;
            ViewBag.EndDate = end;

            var model = await BuildDashboardModelAsync(start, end);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Data(DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.Today;
            var end = endDate ?? DateTime.Today;
            if (start > end) (start, end) = (end, start);

            var model = await BuildDashboardModelAsync(start, end);
            return Json(model);
        }

        private async Task<DashboardViewModel> BuildDashboardModelAsync(DateTime start, DateTime end)
        {
            var chartStart = start.AddDays(-6);

            var summaryTask = _dashboardService.GetSummaryAsync(startDate: start, endDate: end);
            var stationsTask = _stationService.GetAllAsync();
            var salesReportTask = _reportService.GetSalesReportAsync(chartStart, end);
            var performanceTask = _dashboardService.GetStationPerformanceAsync(start, end, 5);

            await Task.WhenAll(summaryTask, stationsTask, salesReportTask, performanceTask);

            var summary = summaryTask.Result;
            var stations = stationsTask.Result;
            var salesReport = salesReportTask.Result;
            var performance = performanceTask.Result;

            var model = new DashboardViewModel
            {
                TotalStations = stations.Data?.Count ?? 0,
                TodaysSales = summary.Data?.TodaysSales ?? 0,
                TransactionCount = summary.Data?.TransactionCount ?? 0,
                LowStockAlerts = summary.Data?.LowStockAlerts?.Count ?? 0,
                AverageSale = summary.Data?.AverageSale ?? 0,
                CreditSales = summary.Data?.CreditSales ?? 0,
                CreditTransactionCount = summary.Data?.CreditTransactionCount ?? 0,
                RecentTransactions = summary.Data?.RecentTransactions?
                    .Select(t => new RecentTransaction
                    {
                        TransactionId = t.ReceiptNumber,
                        Time = t.TransactionDate,
                        StationName = t.StationName,
                        CashierName = t.CashierName,
                        CustomerName = t.CustomerName,
                        FuelType = t.FuelType,
                        Quantity = t.Liters,
                        PaymentMethod = t.PaymentMethod,
                        Total = t.Total
                    }).ToList() ?? new(),
                TopStations = performance.Data?.Select(p => new StationPerformance
                {
                    StationId = p.StationId,
                    StationName = p.StationName,
                    TotalSales = p.TotalSales,
                    TransactionCount = p.TransactionCount,
                    TotalLiters = p.TotalLiters,
                    CashSales = p.CashSales,
                    CreditSales = p.CreditSales,
                    Rank = p.Rank
                }).ToList() ?? new()
            };

            // Sales chart from report
            if (salesReport.Data?.DailySales != null)
            {
                model.SalesChart = salesReport.Data.DailySales
                    .Select(d => new DailySales
                    {
                        Date = d.Date.ToString("dd MMM"),
                        Sales = d.Amount
                    }).ToList();
            }

            // Fuel type chart from report
            if (salesReport.Data?.SalesByFuel != null)
            {
                model.FuelTypeChart = salesReport.Data.SalesByFuel
                    .Select(f => new FuelSalesData
                    {
                        FuelType = f.FuelType,
                        Amount = f.Amount
                    }).ToList();
            }

            return model;
        }
    }
}
