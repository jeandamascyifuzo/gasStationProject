using Escale.Web.Helpers;
using Escale.Web.Models;
using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly IApiTransactionService _transactionService;
        private readonly IApiStationService _stationService;
        private readonly IApiFuelTypeService _fuelTypeService;
        private readonly IApiReportService _reportService;

        public TransactionsController(
            IApiTransactionService transactionService,
            IApiStationService stationService,
            IApiFuelTypeService fuelTypeService,
            IApiReportService reportService)
        {
            _transactionService = transactionService;
            _stationService = stationService;
            _fuelTypeService = fuelTypeService;
            _reportService = reportService;
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _transactionService.GetByIdAsync(id);

            if (!result.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = result.Message ?? "Transaction not found.";
                return RedirectToAction("Index");
            }

            var t = result.Data;
            var model = new Transaction
            {
                Id = t.Id,
                ReceiptNumber = t.ReceiptNumber,
                StationName = t.StationName,
                FuelType = t.FuelType,
                Liters = t.Liters,
                PricePerLiter = t.PricePerLiter,
                Subtotal = t.Subtotal,
                VAT = t.VAT,
                Total = t.Total,
                PaymentMethod = t.PaymentMethod,
                CustomerName = t.CustomerName,
                EBMCode = t.EBMReceiptUrl,
                EBMSent = t.EBMSent,
                CashierName = t.CashierName,
                TransactionDate = t.TransactionDate,
                Status = t.Status
            };

            return View(model);
        }

        public async Task<IActionResult> Index(TransactionFilterViewModel filter)
        {
            // Supervisor: force-filter to assigned stations
            var userRole = HttpContext.Session.GetString(TokenHelper.SessionUserRole);
            HashSet<Guid>? assignedStationIds = null;
            if (userRole == "Supervisor")
            {
                var stationIdsStr = HttpContext.Session.GetString(TokenHelper.SessionStationIds) ?? "";
                assignedStationIds = stationIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
                    .Where(g => g != Guid.Empty).ToHashSet();

                // If supervisor hasn't selected a station, default to first assigned
                if (!filter.StationId.HasValue && assignedStationIds.Count == 1)
                    filter.StationId = assignedStationIds.First();
                // If supervisor selected a station not assigned to them, reject
                if (filter.StationId.HasValue && !assignedStationIds.Contains(filter.StationId.Value))
                    filter.StationId = assignedStationIds.First();
            }

            var apiFilter = new TransactionFilterDto
            {
                StationId = filter.StationId,
                StartDate = filter.StartDate ?? DateTime.Today,
                EndDate = filter.EndDate ?? DateTime.Today,
                FuelTypeId = filter.FuelTypeId,
                PaymentMethod = filter.PaymentMethod,
                Page = filter.Page > 0 ? filter.Page : 1,
                PageSize = filter.PageSize > 0 ? filter.PageSize : 50
            };

            var transactionsTask = _transactionService.GetAllAsync(apiFilter);
            var stationsTask = _stationService.GetAllAsync();
            var fuelTypesTask = _fuelTypeService.GetAllAsync();

            await Task.WhenAll(transactionsTask, stationsTask, fuelTypesTask);

            var transactions = transactionsTask.Result;
            var stations = stationsTask.Result;
            var fuelTypes = fuelTypesTask.Result;

            var model = new TransactionFilterViewModel
            {
                StationId = filter.StationId,
                StartDate = apiFilter.StartDate,
                EndDate = apiFilter.EndDate,
                FuelTypeId = filter.FuelTypeId,
                PaymentMethod = filter.PaymentMethod,
                Page = apiFilter.Page,
                PageSize = apiFilter.PageSize,
                TotalCount = transactions.Data?.TotalCount ?? 0,
                TotalPages = transactions.Data?.TotalPages ?? 0,
                Transactions = transactions.Data?.Items?.Select(t => new Transaction
                {
                    Id = t.Id,
                    ReceiptNumber = t.ReceiptNumber,
                    StationName = t.StationName,
                    FuelType = t.FuelType,
                    Liters = t.Liters,
                    PricePerLiter = t.PricePerLiter,
                    Subtotal = t.Subtotal,
                    VAT = t.VAT,
                    Total = t.Total,
                    PaymentMethod = t.PaymentMethod,
                    CustomerName = t.CustomerName,
                    EBMCode = t.EBMReceiptUrl,
                    EBMSent = t.EBMSent,
                    CashierName = t.CashierName,
                    TransactionDate = t.TransactionDate,
                    Status = t.Status
                }).ToList() ?? new(),
                Stations = (stations.Data?.Select(s => new Station
                {
                    Id = s.Id,
                    Name = s.Name
                }) ?? Enumerable.Empty<Station>())
                .Where(s => assignedStationIds == null || assignedStationIds.Contains(s.Id))
                .ToList(),
                FuelTypes = fuelTypes.Data?.Select(f => new FuelType
                {
                    Id = f.Id,
                    Name = f.Name
                }).ToList() ?? new()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(DateTime? startDate, DateTime? endDate,
            Guid? stationId = null, Guid? fuelTypeId = null, string? paymentMethod = null)
        {
            var start = startDate ?? DateTime.Today;
            var end = endDate ?? DateTime.Today;

            var data = await _reportService.ExportTransactionsAsync(start, end);
            if (data == null)
                return NotFound("No data to export.");

            return File(data, "text/csv", $"transactions_{start:yyyyMMdd}_{end:yyyyMMdd}.csv");
        }
    }
}
