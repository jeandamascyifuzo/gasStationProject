using Escale.Web.Models;
using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class InventoryController : Controller
    {
        private readonly IApiInventoryService _inventoryService;
        private readonly IApiStationService _stationService;
        private readonly IApiFuelTypeService _fuelTypeService;

        public InventoryController(
            IApiInventoryService inventoryService,
            IApiStationService stationService,
            IApiFuelTypeService fuelTypeService)
        {
            _inventoryService = inventoryService;
            _stationService = stationService;
            _fuelTypeService = fuelTypeService;
        }

        public async Task<IActionResult> Index(Guid? stationId = null)
        {
            var inventoryTask = _inventoryService.GetAllAsync(stationId);
            var refillsTask = _inventoryService.GetRefillsAsync(10);
            var stationsTask = _stationService.GetAllAsync();
            var fuelTypesTask = _fuelTypeService.GetAllAsync();

            await Task.WhenAll(inventoryTask, refillsTask, stationsTask, fuelTypesTask);

            var inventory = inventoryTask.Result;
            var refills = refillsTask.Result;
            var stations = stationsTask.Result;
            var fuelTypes = fuelTypesTask.Result;

            var items = inventory.Data?.Select(i => new InventoryItem
            {
                Id = i.Id,
                StationId = i.StationId,
                StationName = i.StationName,
                FuelTypeId = i.FuelTypeId,
                FuelType = i.FuelType,
                CurrentLevel = i.CurrentLevel,
                Capacity = i.Capacity,
                ReorderLevel = i.ReorderLevel,
                PercentageFull = i.PercentageFull,
                LastRefill = i.LastRefill,
                NextDeliveryDate = i.NextDeliveryDate,
                Status = i.Status
            }).ToList() ?? new();

            var model = new InventoryViewModel
            {
                InventoryItems = items,
                RecentRefills = refills.Data?.Select(r => new RefillRecord
                {
                    Id = r.Id,
                    StationName = r.StationName,
                    FuelType = r.FuelType,
                    Quantity = r.Quantity,
                    UnitCost = r.UnitCost,
                    TotalCost = r.TotalCost,
                    RefillDate = r.RefillDate,
                    SupplierName = r.SupplierName,
                    InvoiceNumber = r.InvoiceNumber,
                    RecordedBy = r.RecordedBy
                }).ToList() ?? new(),
                Stations = stations.Data?.Select(s => new Station
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList() ?? new(),
                FuelTypes = fuelTypes.Data?.Select(f => new FuelType
                {
                    Id = f.Id,
                    Name = f.Name
                }).ToList() ?? new(),
                Stats = new InventoryStats
                {
                    TotalItems = items.Count,
                    LowStockItems = items.Count(i => i.Status == "Low"),
                    CriticalItems = items.Count(i => i.Status == "Critical"),
                    TotalCapacity = items.Sum(i => i.Capacity),
                    TotalCurrentStock = items.Sum(i => i.CurrentLevel)
                },
                SelectedStationId = stationId
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RecordRefill(Guid inventoryItemId, decimal quantity, decimal unitCost, string? supplierName, string? invoiceNumber, DateTime refillDate)
        {
            var request = new CreateRefillRequestDto
            {
                InventoryItemId = inventoryItemId,
                Quantity = quantity,
                UnitCost = unitCost,
                SupplierName = supplierName,
                InvoiceNumber = invoiceNumber,
                RefillDate = refillDate
            };

            var result = await _inventoryService.CreateRefillAsync(request);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Refill recorded successfully!" : result.Message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReorderLevel(Guid id, decimal reorderLevel)
        {
            var request = new UpdateReorderLevelRequestDto
            {
                Id = id,
                ReorderLevel = reorderLevel
            };

            var result = await _inventoryService.UpdateReorderLevelAsync(request);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Reorder level updated!" : result.Message;

            return RedirectToAction("Index");
        }
    }
}
