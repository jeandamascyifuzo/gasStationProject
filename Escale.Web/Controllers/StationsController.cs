using Escale.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class StationsController : Controller
    {
        public IActionResult Index()
        {
            // TODO: Replace with actual data from database
            var model = new StationViewModel
            {
                Stations = new List<Station>
                {
                    new() { Id = 1, Name = "Kigali Central Station", Location = "Kigali", Address = "KN 4 Ave", ContactNumber = "+250788123456", ManagerName = "John Doe", Status = "Active", PumpCount = 4 },
                    new() { Id = 2, Name = "Remera Branch", Location = "Remera", Address = "KG 11 Ave", ContactNumber = "+250788234567", ManagerName = "Jane Smith", Status = "Active", PumpCount = 6 },
                    new() { Id = 3, Name = "Kimironko Station", Location = "Kimironko", Address = "KG 5 Ave", ContactNumber = "+250788345678", ManagerName = "Bob Johnson", Status = "Active", PumpCount = 3 }
                },
                Managers = new List<User>
                {
                    new() { Id = 1, Name = "John Doe", Email = "john@escale.rw", Role = "Manager" },
                    new() { Id = 2, Name = "Jane Smith", Email = "jane@escale.rw", Role = "Manager" },
                    new() { Id = 3, Name = "Bob Johnson", Email = "bob@escale.rw", Role = "Manager" }
                }
            };

            return View(model);
        }

        public IActionResult Details(int id)
        {
            // TODO: Replace with actual data from database
            var station = new Station
            {
                Id = id,
                Name = "Kigali Central Station",
                Location = "Kigali",
                Address = "KN 4 Ave",
                ContactNumber = "+250788123456",
                ManagerId = 1,
                ManagerName = "John Doe",
                Status = "Active",
                PumpCount = 4
            };

            var sales = new List<StationSale>
            {
                new() { Date = DateTime.Today, FuelType = "Petrol", Quantity = 1200, TotalSales = 1800000, TransactionCount = 45 },
                new() { Date = DateTime.Today, FuelType = "Diesel", Quantity = 800, TotalSales = 1200000, TransactionCount = 32 },
                new() { Date = DateTime.Today.AddDays(-1), FuelType = "Petrol", Quantity = 1150, TotalSales = 1725000, TransactionCount = 42 },
                new() { Date = DateTime.Today.AddDays(-1), FuelType = "Diesel", Quantity = 750, TotalSales = 1125000, TransactionCount = 28 }
            };

            var stock = new List<StationStock>
            {
                new() { FuelType = "Petrol", CurrentLevel = 15000, Capacity = 20000, ReorderLevel = 5000, LastRefill = DateTime.Now.AddDays(-3) },
                new() { FuelType = "Diesel", CurrentLevel = 8000, Capacity = 15000, ReorderLevel = 3000, LastRefill = DateTime.Now.AddDays(-5) },
                new() { FuelType = "Super", CurrentLevel = 3000, Capacity = 10000, ReorderLevel = 2000, LastRefill = DateTime.Now.AddDays(-7) }
            };

            var employees = new List<User>
            {
                new() { Id = 1, Name = "John Doe", Email = "john@escale.rw", Role = "Manager", Phone = "+250788123456", IsActive = true },
                new() { Id = 2, Name = "Alice Johnson", Email = "alice@escale.rw", Role = "Cashier", Phone = "+250788234567", IsActive = true },
                new() { Id = 3, Name = "Bob Smith", Email = "bob@escale.rw", Role = "Cashier", Phone = "+250788345678", IsActive = true },
                new() { Id = 4, Name = "Carol White", Email = "carol@escale.rw", Role = "Pump Attendant", Phone = "+250788456789", IsActive = false }
            };

            var stats = new StationStats
            {
                TodaysSales = 3000000,
                TodaysTransactions = 77,
                TotalEmployees = 4,
                ActivePumps = 4,
                LowStockItems = 1
            };

            var model = new StationDetailsViewModel
            {
                Station = station,
                Sales = sales,
                Stock = stock,
                Employees = employees,
                Stats = stats
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(Station station)
        {
            // TODO: Save to database
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(Station station)
        {
            // TODO: Update in database
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            // TODO: Delete from database
            return RedirectToAction("Index");
        }
    }
}
