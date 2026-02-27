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

        public async Task<IActionResult> Index(string period = "today")
        {
            // SuperAdmin is platform-level, redirect to Organizations
            if (HttpContext.Session.GetString("UserRole") == "SuperAdmin")
                return RedirectToAction("Index", "Organizations");

            DateTime date = DateTime.Today;
            DateTime chartStart = DateTime.Today.AddDays(-6);

            if (period == "week")
            {
                date = DateTime.Today.AddDays(-7);
                chartStart = DateTime.Today.AddDays(-13);
            }
            else if (period == "month")
            {
                date = DateTime.Today.AddDays(-30);
                chartStart = DateTime.Today.AddDays(-30);
            }

            ViewBag.Period = period;

            var summaryTask = _dashboardService.GetSummaryAsync(date: date);
            var stationsTask = _stationService.GetAllAsync();
            var salesReportTask = _reportService.GetSalesReportAsync(
                chartStart, DateTime.Today);

            await Task.WhenAll(summaryTask, stationsTask, salesReportTask);

            var summary = summaryTask.Result;
            var stations = stationsTask.Result;
            var salesReport = salesReportTask.Result;

            var model = new DashboardViewModel
            {
                TotalStations = stations.Data?.Count ?? 0,
                TodaysSales = summary.Data?.TodaysSales ?? 0,
                TransactionCount = summary.Data?.TransactionCount ?? 0,
                LowStockAlerts = summary.Data?.LowStockAlerts?.Count ?? 0,
                AverageSale = summary.Data?.AverageSale ?? 0,
                RecentTransactions = summary.Data?.RecentTransactions?
                    .Select(t => new RecentTransaction
                    {
                        TransactionId = t.ReceiptNumber,
                        FuelType = t.FuelType,
                        Quantity = t.Liters,
                        Total = t.Total,
                        Time = t.TransactionDate
                    }).ToList() ?? new()
            };

            // Sales chart from report
            if (salesReport.Data?.DailySales != null)
            {
                model.SalesChart = salesReport.Data.DailySales
                    .Select(d => new DailySales
                    {
                        Date = d.Date.ToString("ddd"),
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

            return View(model);
        }
    }
}
