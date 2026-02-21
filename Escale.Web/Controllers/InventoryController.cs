using Escale.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class InventoryController : Controller
    {
        public IActionResult Index()
        {
            // TODO: Replace with actual data from database
            var model = new InventoryViewModel
            {
                InventoryItems = new List<InventoryItem>
                {
                    new() 
                    { 
                        Id = 1, 
                        StationId = 1, 
                        StationName = "Kigali Central Station", 
                        FuelTypeId = 1, 
                        FuelType = "Petrol", 
                        CurrentLevel = 15000, 
                        Capacity = 20000, 
                        ReorderLevel = 5000,
                        LastRefill = DateTime.Now.AddDays(-3),
                        LastRefillQuantity = 8000,
                        NextDeliveryDate = DateTime.Now.AddDays(2),
                        Status = "Normal"
                    },
                    new() 
                    { 
                        Id = 2, 
                        StationId = 1, 
                        StationName = "Kigali Central Station", 
                        FuelTypeId = 2, 
                        FuelType = "Diesel", 
                        CurrentLevel = 8000, 
                        Capacity = 15000, 
                        ReorderLevel = 3000,
                        LastRefill = DateTime.Now.AddDays(-5),
                        LastRefillQuantity = 6000,
                        NextDeliveryDate = DateTime.Now.AddDays(1),
                        Status = "Normal"
                    },
                    new() 
                    { 
                        Id = 3, 
                        StationId = 1, 
                        StationName = "Kigali Central Station", 
                        FuelTypeId = 3, 
                        FuelType = "Super", 
                        CurrentLevel = 2000, 
                        Capacity = 10000, 
                        ReorderLevel = 2500,
                        LastRefill = DateTime.Now.AddDays(-7),
                        LastRefillQuantity = 5000,
                        Status = "Low Stock"
                    },
                    new() 
                    { 
                        Id = 4, 
                        StationId = 2, 
                        StationName = "Remera Branch", 
                        FuelTypeId = 1, 
                        FuelType = "Petrol", 
                        CurrentLevel = 18000, 
                        Capacity = 25000, 
                        ReorderLevel = 6000,
                        LastRefill = DateTime.Now.AddDays(-2),
                        LastRefillQuantity = 10000,
                        Status = "Normal"
                    },
                    new() 
                    { 
                        Id = 5, 
                        StationId = 2, 
                        StationName = "Remera Branch", 
                        FuelTypeId = 2, 
                        FuelType = "Diesel", 
                        CurrentLevel = 2500, 
                        Capacity = 20000, 
                        ReorderLevel = 5000,
                        LastRefill = DateTime.Now.AddDays(-8),
                        LastRefillQuantity = 7000,
                        NextDeliveryDate = DateTime.Now,
                        Status = "Critical"
                    }
                },
                RecentRefills = new List<RefillRecord>
                {
                    new()
                    {
                        Id = 1,
                        StationId = 1,
                        StationName = "Kigali Central Station",
                        FuelType = "Petrol",
                        Quantity = 8000,
                        UnitCost = 1200,
                        TotalCost = 9600000,
                        RefillDate = DateTime.Now.AddDays(-3),
                        SupplierName = "Rwanda Fuel Ltd",
                        InvoiceNumber = "INV-2024-001",
                        RecordedBy = "John Doe"
                    },
                    new()
                    {
                        Id = 2,
                        StationId = 2,
                        StationName = "Remera Branch",
                        FuelType = "Petrol",
                        Quantity = 10000,
                        UnitCost = 1200,
                        TotalCost = 12000000,
                        RefillDate = DateTime.Now.AddDays(-2),
                        SupplierName = "Rwanda Fuel Ltd",
                        InvoiceNumber = "INV-2024-002",
                        RecordedBy = "Jane Smith"
                    },
                    new()
                    {
                        Id = 3,
                        StationId = 1,
                        StationName = "Kigali Central Station",
                        FuelType = "Diesel",
                        Quantity = 6000,
                        UnitCost = 1300,
                        TotalCost = 7800000,
                        RefillDate = DateTime.Now.AddDays(-5),
                        SupplierName = "Total Energy",
                        InvoiceNumber = "INV-2024-003",
                        RecordedBy = "John Doe"
                    }
                },
                Stations = new List<Station>
                {
                    new() { Id = 1, Name = "Kigali Central Station" },
                    new() { Id = 2, Name = "Remera Branch" },
                    new() { Id = 3, Name = "Kimironko Station" }
                },
                FuelTypes = new List<FuelType>
                {
                    new() { Id = 1, Name = "Petrol", Code = "PET" },
                    new() { Id = 2, Name = "Diesel", Code = "DSL" },
                    new() { Id = 3, Name = "Super", Code = "SUP" }
                },
                Stats = new InventoryStats
                {
                    TotalItems = 5,
                    LowStockItems = 1,
                    CriticalItems = 1,
                    TotalCapacity = 90000,
                    TotalCurrentStock = 45500
                }
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult RecordRefill(RefillRecord refill)
        {
            // TODO: Save refill to database
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateStock(int id, decimal quantity)
        {
            // TODO: Update stock in database
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateReorderLevel(int id, decimal reorderLevel)
        {
            // TODO: Update reorder level in database
            return RedirectToAction("Index");
        }
    }
}
