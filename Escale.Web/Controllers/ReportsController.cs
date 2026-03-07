using System.Text;
using Escale.Web.Models;
using Escale.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IApiReportService _reportService;
        private readonly IApiStationService _stationService;
        private readonly IApiFuelTypeService _fuelTypeService;

        public ReportsController(
            IApiReportService reportService,
            IApiStationService stationService,
            IApiFuelTypeService fuelTypeService)
        {
            _reportService = reportService;
            _stationService = stationService;
            _fuelTypeService = fuelTypeService;
        }

        public async Task<IActionResult> Index()
        {
            var stationsTask = _stationService.GetAllAsync();
            var fuelTypesTask = _fuelTypeService.GetAllAsync();

            await Task.WhenAll(stationsTask, fuelTypesTask);

            var model = new ReportViewModel
            {
                Stations = stationsTask.Result.Data?.Select(s => new Station
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList() ?? new(),
                FuelTypes = fuelTypesTask.Result.Data?.Select(f => new FuelType
                {
                    Id = f.Id,
                    Name = f.Name
                }).ToList() ?? new(),
                StartDate = DateTime.Today,
                EndDate = DateTime.Today
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateSalesReport(DateTime? startDate, DateTime? endDate, Guid? stationId)
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;
            var result = await _reportService.GetSalesReportAsync(start, end, stationId);
            return Json(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateInventoryReport(Guid? stationId)
        {
            var result = await _reportService.GetInventoryReportAsync(stationId);
            return Json(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateEmployeeReport(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;
            var result = await _reportService.GetEmployeeReportAsync(start, end);
            return Json(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateCustomerReport(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;
            var result = await _reportService.GetCustomerReportAsync(start, end);
            return Json(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateFinancialReport(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;
            var result = await _reportService.GetFinancialReportAsync(start, end);
            return Json(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> ExportReport(string reportType, DateTime? startDate, DateTime? endDate, Guid? stationId)
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;
            var filename = $"{reportType}_report_{start:yyyyMMdd}_{end:yyyyMMdd}.csv";

            switch (reportType)
            {
                case "Sales":
                {
                    var result = await _reportService.GetSalesReportAsync(start, end, stationId);
                    if (result.Data == null) return NotFound("No data");
                    var d = result.Data;
                    var sb = new StringBuilder();
                    sb.AppendLine("Metric,Value");
                    sb.AppendLine($"Period,{d.StartDate:yyyy-MM-dd} to {d.EndDate:yyyy-MM-dd}");
                    sb.AppendLine($"Total Sales,{d.TotalSales}");
                    sb.AppendLine($"Transaction Count,{d.TransactionCount}");
                    sb.AppendLine();
                    sb.AppendLine("Fuel Type,Liters,Amount,Transactions");
                    foreach (var f in d.SalesByFuel)
                        sb.AppendLine($"{f.FuelType},{f.Liters},{f.Amount},{f.TransactionCount}");
                    sb.AppendLine();
                    sb.AppendLine("Payment Method,Amount,Transactions");
                    foreach (var p in d.SalesByPayment)
                        sb.AppendLine($"{p.PaymentMethod},{p.Amount},{p.TransactionCount}");
                    sb.AppendLine();
                    sb.AppendLine("Date,Amount,Transactions");
                    foreach (var ds in d.DailySales)
                        sb.AppendLine($"{ds.Date:yyyy-MM-dd},{ds.Amount},{ds.TransactionCount}");
                    return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", filename);
                }

                case "Inventory":
                {
                    var result = await _reportService.GetInventoryReportAsync(stationId);
                    if (result.Data == null || result.Data.Count == 0) return NotFound("No data");
                    var sb = new StringBuilder();
                    sb.AppendLine("Station,Fuel Type,Current Level,Capacity,% Full,Status,Refill Count,Refill Cost");
                    foreach (var i in result.Data)
                        sb.AppendLine($"{i.StationName},{i.FuelType},{i.CurrentLevel},{i.Capacity},{i.PercentageFull},{i.Status},{i.RefillCount},{i.TotalRefillCost}");
                    return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", filename);
                }

                case "Employee":
                {
                    var result = await _reportService.GetEmployeeReportAsync(start, end);
                    if (result.Data == null || result.Data.Count == 0) return NotFound("No data");
                    var sb = new StringBuilder();
                    sb.AppendLine("Employee,Role,Transactions,Total Sales,Shifts,Hours Worked");
                    foreach (var e in result.Data)
                        sb.AppendLine($"{e.FullName},{e.Role},{e.TransactionCount},{e.TotalSales},{e.ShiftCount},{e.TotalHoursWorked:F1}");
                    return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", filename);
                }

                case "Customer":
                {
                    var result = await _reportService.GetCustomerReportAsync(start, end);
                    if (result.Data == null || result.Data.Count == 0) return NotFound("No data");
                    var sb = new StringBuilder();
                    sb.AppendLine("Customer,Type,Status,Phone,Email,TIN,Cars,Subscriptions,Transactions,Total Spent,Total Liters,Remaining Balance");
                    foreach (var c in result.Data)
                        sb.AppendLine($"\"{c.Name}\",{c.Type},{(c.IsActive ? "Active" : "Inactive")},{c.PhoneNumber},{c.Email},{c.TIN},{c.CarCount},{c.SubscriptionCount},{c.TransactionCount},{c.TotalSpent},{c.TotalLiters},{c.SubscriptionBalance}");
                    return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", filename);
                }

                case "Financial":
                {
                    var result = await _reportService.GetFinancialReportAsync(start, end);
                    if (result.Data == null) return NotFound("No data");
                    var d = result.Data;
                    var sb = new StringBuilder();
                    sb.AppendLine("Metric,Value");
                    sb.AppendLine($"Period,{d.StartDate:yyyy-MM-dd} to {d.EndDate:yyyy-MM-dd}");
                    sb.AppendLine($"Total Revenue,{d.TotalRevenue}");
                    sb.AppendLine($"Total VAT,{d.TotalVAT}");
                    sb.AppendLine($"Total Refill Cost,{d.TotalRefillCost}");
                    sb.AppendLine($"Gross Profit,{d.GrossProfit}");
                    sb.AppendLine();
                    sb.AppendLine("Date,Amount,Transactions");
                    foreach (var dr in d.DailyRevenue)
                        sb.AppendLine($"{dr.Date:yyyy-MM-dd},{dr.Amount},{dr.TransactionCount}");
                    return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", filename);
                }

                default:
                {
                    var data = await _reportService.ExportTransactionsAsync(start, end);
                    if (data == null) return NotFound("No data");
                    return File(data, "text/csv", filename);
                }
            }
        }
    }
}
