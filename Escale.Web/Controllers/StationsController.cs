using Escale.Web.Helpers;
using Escale.Web.Models;
using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class StationsController : Controller
    {
        private readonly IApiStationService _stationService;
        private readonly IApiUserService _userService;
        private readonly IApiInventoryService _inventoryService;
        private readonly IApiReportService _reportService;

        public StationsController(
            IApiStationService stationService,
            IApiUserService userService,
            IApiInventoryService inventoryService,
            IApiReportService reportService)
        {
            _stationService = stationService;
            _userService = userService;
            _inventoryService = inventoryService;
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var stationsTask = _stationService.GetAllAsync();
            var usersTask = _userService.GetAllAsync(1, 100);

            await Task.WhenAll(stationsTask, usersTask);

            var stations = stationsTask.Result;
            var users = usersTask.Result;

            var managers = users.Data?.Items?
                .Where(u => u.Role == "Manager" || u.Role == "Admin")
                .Select(u => new User
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role
                }).ToList() ?? new();

            var allStations = stations.Data?.Select(s => new Station
            {
                Id = s.Id,
                Name = s.Name,
                Location = s.Location,
                Address = s.Address,
                ContactNumber = s.PhoneNumber,
                ManagerId = s.ManagerId,
                ManagerName = s.Manager,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            }).ToList() ?? new();

            // Supervisor can only see their assigned stations
            var userRole = HttpContext.Session.GetString(TokenHelper.SessionUserRole);
            if (userRole == "Supervisor")
            {
                var stationIdsStr = HttpContext.Session.GetString(TokenHelper.SessionStationIds) ?? "";
                var assignedIds = stationIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
                    .Where(g => g != Guid.Empty).ToHashSet();
                allStations = allStations.Where(s => assignedIds.Contains(s.Id)).ToList();
            }

            var model = new StationViewModel
            {
                Stations = allStations,
                Managers = managers
            };

            return View(model);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            // Supervisor can only view their assigned stations
            var userRole = HttpContext.Session.GetString(TokenHelper.SessionUserRole);
            if (userRole == "Supervisor")
            {
                var stationIdsStr = HttpContext.Session.GetString(TokenHelper.SessionStationIds) ?? "";
                var assignedIds = stationIdsStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(sid => Guid.TryParse(sid, out var g) ? g : Guid.Empty).ToHashSet();
                if (!assignedIds.Contains(id))
                    return RedirectToAction("Index");
            }

            var stationTask = _stationService.GetByIdAsync(id);
            var inventoryTask = _inventoryService.GetAllAsync(id);
            var usersTask = _userService.GetAllAsync(1, 100);

            await Task.WhenAll(stationTask, inventoryTask, usersTask);

            var stationResult = stationTask.Result;
            var inventoryResult = inventoryTask.Result;
            var usersResult = usersTask.Result;

            if (!stationResult.Success || stationResult.Data == null)
            {
                TempData["ErrorMessage"] = stationResult.Message;
                return RedirectToAction("Index");
            }

            var s = stationResult.Data;
            var station = new Station
            {
                Id = s.Id,
                Name = s.Name,
                Location = s.Location,
                Address = s.Address,
                ContactNumber = s.PhoneNumber,
                ManagerId = s.ManagerId,
                ManagerName = s.Manager,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            };

            var stock = inventoryResult.Data?.Select(i => new StationStock
            {
                InventoryItemId = i.Id,
                FuelType = i.FuelType,
                CurrentLevel = i.CurrentLevel,
                Capacity = i.Capacity,
                ReorderLevel = i.ReorderLevel,
                LastRefill = i.LastRefill,
                PercentageFull = i.PercentageFull
            }).ToList() ?? new();

            var employees = usersResult.Data?.Items?
                .Where(u => u.AssignedStations.Any(st => st.Id == id))
                .Select(u => new User
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role,
                    IsActive = u.IsActive
                }).ToList() ?? new();

            var model = new StationDetailsViewModel
            {
                Station = station,
                Stock = stock,
                Employees = employees,
                Stats = new StationStats
                {
                    TodaysSales = s.TodaySales,
                    TodaysTransactions = s.TodayTransactionCount,
                    TotalEmployees = s.EmployeeCount,
                    LowStockItems = stock.Count(st => st.IsLowStock)
                }
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetStationSales(Guid id, DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today;
            var end = endDate ?? DateTime.Today;
            var result = await _reportService.GetSalesReportAsync(start, end, id);
            return Json(result.Data);
        }
    }
}
