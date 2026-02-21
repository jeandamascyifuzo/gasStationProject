using Escale.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            var model = new ReportViewModel
            {
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
                StartDate = DateTime.Today.AddDays(-30),
                EndDate = DateTime.Today
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult GenerateSalesReport(DateTime? startDate, DateTime? endDate, int? stationId, int? fuelTypeId)
        {
            // TODO: Generate actual sales report from database
            var salesData = new List<SalesReportData>
            {
                new()
                {
                    Date = DateTime.Today,
                    Station = "Kigali Central Station",
                    FuelType = "Petrol",
                    Quantity = 1200,
                    TotalSales = 1800000,
                    TransactionCount = 45,
                    AveragePerTransaction = 40000
                },
                new()
                {
                    Date = DateTime.Today,
                    Station = "Kigali Central Station",
                    FuelType = "Diesel",
                    Quantity = 800,
                    TotalSales = 1200000,
                    TransactionCount = 32,
                    AveragePerTransaction = 37500
                }
            };

            return Json(salesData);
        }

        [HttpGet]
        public IActionResult GenerateInventoryReport(DateTime? startDate, DateTime? endDate, int? stationId)
        {
            // TODO: Generate actual inventory report from database
            var inventoryData = new List<InventoryReportData>
            {
                new()
                {
                    Station = "Kigali Central Station",
                    FuelType = "Petrol",
                    OpeningStock = 10000,
                    Received = 8000,
                    Sold = 3000,
                    ClosingStock = 15000,
                    Variance = 0
                }
            };

            return Json(inventoryData);
        }

        [HttpGet]
        public IActionResult GenerateEmployeeReport(DateTime? startDate, DateTime? endDate, int? stationId)
        {
            // TODO: Generate actual employee report from database
            var employeeData = new List<EmployeeReportData>
            {
                new()
                {
                    Date = DateTime.Today,
                    EmployeeName = "Alice Johnson",
                    Role = "Cashier",
                    Station = "Kigali Central Station",
                    TransactionsProcessed = 45,
                    TotalSales = 1800000
                }
            };

            return Json(employeeData);
        }

        [HttpGet]
        public IActionResult GenerateCustomerReport(DateTime? startDate, DateTime? endDate)
        {
            // TODO: Generate actual customer report from database
            var customerData = new List<CustomerReportData>
            {
                new()
                {
                    CustomerName = "Jean Dupont",
                    CustomerType = "Individual",
                    TotalCars = 2,
                    ActiveSubscriptions = 1,
                    TotalSpent = 450000,
                    TransactionCount = 25,
                    LastTransaction = DateTime.Now.AddDays(-2)
                }
            };

            return Json(customerData);
        }

        [HttpGet]
        public IActionResult GenerateFinancialReport(DateTime? startDate, DateTime? endDate)
        {
            // TODO: Generate actual financial report from database
            var financialData = new List<FinancialReportData>
            {
                new()
                {
                    Date = DateTime.Today,
                    TotalRevenue = 3000000,
                    CostOfGoods = 2100000,
                    GrossProfit = 900000,
                    Expenses = 300000,
                    NetProfit = 600000,
                    ProfitMargin = 20
                }
            };

            return Json(financialData);
        }

        [HttpGet]
        public IActionResult ExportReport(string reportType, DateTime? startDate, DateTime? endDate)
        {
            // TODO: Implement export functionality (CSV, PDF, Excel)
            return RedirectToAction("Index");
        }
    }
}
