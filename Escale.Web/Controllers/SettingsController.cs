using Escale.Web.Models;
using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class SettingsController : Controller
    {
        private readonly IApiSettingsService _settingsService;
        private readonly IApiFuelTypeService _fuelTypeService;

        public SettingsController(IApiSettingsService settingsService, IApiFuelTypeService fuelTypeService)
        {
            _settingsService = settingsService;
            _fuelTypeService = fuelTypeService;
        }

        public async Task<IActionResult> Index()
        {
            var settingsTask = _settingsService.GetSettingsAsync();
            var ebmTask = _settingsService.GetEbmStatusAsync();
            var ebmConfigTask = _settingsService.GetEbmConfigAsync();
            var fuelTypesTask = _fuelTypeService.GetAllAsync();
            var logoTask = _settingsService.GetLogoUrlAsync();
            var paymentMethodsTask = _settingsService.GetPaymentMethodsAsync();

            await Task.WhenAll(settingsTask, ebmTask, ebmConfigTask, fuelTypesTask, logoTask, paymentMethodsTask);

            var settings = settingsTask.Result;
            var ebm = ebmTask.Result;
            var ebmConfig = ebmConfigTask.Result;
            var fuelTypes = fuelTypesTask.Result;
            var logo = logoTask.Result;
            var paymentMethods = paymentMethodsTask.Result;

            var s = settings.Data ?? new AppSettingsResponseDto();
            var ec = ebmConfig.Data;
            var model = new SettingsViewModel
            {
                LogoUrl = logo.Data,
                CompanyName = s.CompanyName,
                TaxRate = s.TaxRate,
                Currency = s.Currency,
                ReceiptHeader = s.ReceiptHeader,
                ReceiptFooter = s.ReceiptFooter,
                EBMEnabled = s.EBMEnabled,
                EBMServerUrl = s.EBMServerUrl,
                AutoPrintReceipt = s.AutoPrintReceipt,
                RequireCustomerInfo = s.RequireCustomerInfo,
                MinimumSaleAmount = s.MinimumSaleAmount,
                MaximumSaleAmount = s.MaximumSaleAmount,
                AllowNegativeStock = s.AllowNegativeStock,
                LowStockThreshold = s.LowStockThreshold,
                CriticalStockThreshold = s.CriticalStockThreshold,
                EBMConnected = ebm.Data?.IsConnected ?? false,
                EBMLastSync = ebm.Data?.LastSyncAt,
                EBMStatus = ebm.Data?.Status ?? "Unknown",
                EBMIsConfigured = ec?.IsConfigured ?? false,
                EBMCompanyName = ec?.EBMCompanyName,
                EBMCompanyAddress = ec?.EBMCompanyAddress,
                EBMCompanyPhone = ec?.EBMCompanyPhone,
                EBMCompanyTIN = ec?.EBMCompanyTIN,
                FuelTypes = fuelTypes.Data?.Select(f => new FuelType
                {
                    Id = f.Id,
                    Name = f.Name,
                    PricePerLiter = f.PricePerLiter,
                    IsActive = f.IsActive
                }).ToList() ?? new(),
                PaymentMethods = paymentMethods.Data ?? new()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSettings(SettingsViewModel model)
        {
            var request = new UpdateSettingsRequestDto
            {
                CompanyName = model.CompanyName,
                TaxRate = model.TaxRate,
                Currency = model.Currency,
                ReceiptHeader = model.ReceiptHeader,
                ReceiptFooter = model.ReceiptFooter,
                EBMEnabled = model.EBMEnabled,
                EBMServerUrl = model.EBMServerUrl,
                AutoPrintReceipt = model.AutoPrintReceipt,
                RequireCustomerInfo = model.RequireCustomerInfo,
                MinimumSaleAmount = model.MinimumSaleAmount,
                MaximumSaleAmount = model.MaximumSaleAmount,
                AllowNegativeStock = model.AllowNegativeStock,
                LowStockThreshold = model.LowStockThreshold,
                CriticalStockThreshold = model.CriticalStockThreshold
            };

            var result = await _settingsService.UpdateSettingsAsync(request);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Settings updated successfully!" : result.Message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TestEbmConnection()
        {
            var result = await _settingsService.TestEbmConnectionAsync();

            if (result.Success && result.Data)
                TempData["SuccessMessage"] = "EBM connection successful!";
            else
                TempData["ErrorMessage"] = "EBM connection failed. Please contact your administrator.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SyncEBM()
        {
            var result = await _settingsService.SyncEbmAsync();
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "EBM synchronized successfully!" : result.Message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UploadLogo(IFormFile logo)
        {
            if (logo == null || logo.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select an image file.";
                return RedirectToAction("Index");
            }

            var result = await _settingsService.UploadLogoAsync(logo);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Logo uploaded successfully!" : result.Message;

            // Update session logo URL so it reflects immediately in the nav
            if (result.Success && !string.IsNullOrEmpty(result.Data))
            {
                HttpContext.Session.SetString("LogoUrl", result.Data);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TogglePaymentMethod(Guid id, bool isEnabled, string? displayName)
        {
            var request = new UpdatePaymentMethodDto { IsEnabled = isEnabled, DisplayName = displayName };
            var result = await _settingsService.UpdatePaymentMethodAsync(id, request);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFuelPrice(Guid id, decimal pricePerLiter)
        {
            var fuelType = await _fuelTypeService.GetByIdAsync(id);
            if (!fuelType.Success || fuelType.Data == null)
            {
                TempData["ErrorMessage"] = "Fuel type not found.";
                return RedirectToAction("Index");
            }

            var request = new UpdateFuelTypeRequestDto
            {
                Name = fuelType.Data.Name,
                PricePerLiter = pricePerLiter,
                IsActive = fuelType.Data.IsActive
            };

            var result = await _fuelTypeService.UpdateAsync(id, request);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Fuel price updated successfully!" : result.Message;

            return RedirectToAction("Index");
        }
    }
}
