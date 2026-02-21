using Escale.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            // TODO: Replace with actual data from database
            var model = new SettingsViewModel
            {
                CompanySettings = new CompanySettings
                {
                    CompanyName = "Escale Gas Management",
                    TIN = "123456789",
                    Email = "info@escale.rw",
                    Phone = "+250788000000",
                    Address = "KN 4 Ave, Kigali, Rwanda",
                    Currency = "RWF",
                    TimeZone = "Africa/Kigali"
                },
                SystemSettings = new SystemSettings
                {
                    MaintenanceMode = false,
                    AutoBackup = true,
                    BackupFrequency = "Daily",
                    SessionTimeout = 30,
                    EnableEBM = true,
                    EBMApiUrl = "https://ebm.obr.gov.rw/api",
                    AllowNegativeStock = false,
                    LowStockThreshold = 20
                },
                NotificationSettings = new NotificationSettings
                {
                    EmailNotifications = true,
                    SMSNotifications = false,
                    LowStockAlerts = true,
                    DailyReports = true,
                    TransactionAlerts = false,
                    NotificationEmail = "alerts@escale.rw"
                },
                FuelPrices = new List<FuelPrice>
                {
                    new() { Id = 1, FuelType = "Petrol", Price = 1450, EffectiveDate = DateTime.Now.AddMonths(-1), IsActive = true },
                    new() { Id = 2, FuelType = "Diesel", Price = 1380, EffectiveDate = DateTime.Now.AddMonths(-1), IsActive = true },
                    new() { Id = 3, FuelType = "Super", Price = 1520, EffectiveDate = DateTime.Now.AddMonths(-1), IsActive = true }
                }
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult UpdateCompanySettings(CompanySettings settings)
        {
            // TODO: Update company settings in database
            TempData["SuccessMessage"] = "Company settings updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateSystemSettings(SystemSettings settings)
        {
            // TODO: Update system settings in database
            TempData["SuccessMessage"] = "System settings updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateNotificationSettings(NotificationSettings settings)
        {
            // TODO: Update notification settings in database
            TempData["SuccessMessage"] = "Notification settings updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateFuelPrice(FuelPrice fuelPrice)
        {
            // TODO: Update fuel price in database
            TempData["SuccessMessage"] = "Fuel price updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RunBackup()
        {
            // TODO: Implement backup functionality
            TempData["SuccessMessage"] = "Backup initiated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult TestNotification(string type)
        {
            // TODO: Send test notification
            TempData["SuccessMessage"] = $"Test {type} notification sent!";
            return RedirectToAction("Index");
        }
    }
}
