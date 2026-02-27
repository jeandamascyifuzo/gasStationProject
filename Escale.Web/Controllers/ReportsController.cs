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
                StartDate = DateTime.Today.AddDays(-30),
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
        public async Task<IActionResult> ExportReport(string reportType, DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            var data = await _reportService.ExportTransactionsAsync(start, end);
            if (data == null)
            {
                TempData["ErrorMessage"] = "Failed to export report.";
                return RedirectToAction("Index");
            }

            return File(data, "text/csv", $"{reportType}_report_{start:yyyyMMdd}_{end:yyyyMMdd}.csv");
        }
    }
}
