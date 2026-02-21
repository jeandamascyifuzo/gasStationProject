using Escale.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // TODO: Replace with actual data from database
            var model = new DashboardViewModel
            {
                TotalStations = 12,
                ActivePumps = 48,
                TodaysSales = 2450000,
                LowStockAlerts = 3,
                SalesChart = new List<DailySales>
                {
                    new() { Date = "Mon", Sales = 350000 },
                    new() { Date = "Tue", Sales = 420000 },
                    new() { Date = "Wed", Sales = 380000 },
                    new() { Date = "Thu", Sales = 450000 },
                    new() { Date = "Fri", Sales = 480000 },
                    new() { Date = "Sat", Sales = 520000 },
                    new() { Date = "Sun", Sales = 490000 }
                },
                FuelTypeChart = new List<FuelSalesData>
                {
                    new() { FuelType = "Petrol", Amount = 1200000 },
                    new() { FuelType = "Diesel", Amount = 950000 },
                    new() { FuelType = "Super", Amount = 300000 }
                },
                TopStations = new List<StationSalesData>
                {
                    new() { StationName = "Kigali Central", Sales = 450000 },
                    new() { StationName = "Remera Branch", Sales = 380000 },
                    new() { StationName = "Kimironko", Sales = 320000 },
                    new() { StationName = "Nyabugogo", Sales = 290000 },
                    new() { StationName = "Gikondo", Sales = 260000 }
                },
                RecentTransactions = new List<RecentTransaction>
                {
                    new() { TransactionId = "TXN001", Station = "Kigali Central", FuelType = "Petrol", Quantity = 45.5m, Total = 65000, Time = DateTime.Now.AddMinutes(-5) },
                    new() { TransactionId = "TXN002", Station = "Remera Branch", FuelType = "Diesel", Quantity = 30.2m, Total = 42000, Time = DateTime.Now.AddMinutes(-12) },
                    new() { TransactionId = "TXN003", Station = "Kimironko", FuelType = "Super", Quantity = 25.0m, Total = 38000, Time = DateTime.Now.AddMinutes(-18) }
                }
            };

            return View(model);
        }
    }
}
