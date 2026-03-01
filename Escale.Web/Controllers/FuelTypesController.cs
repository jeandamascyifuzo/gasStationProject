using Escale.Web.Models;
using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class FuelTypesController : Controller
    {
        private readonly IApiFuelTypeService _fuelTypeService;

        public FuelTypesController(IApiFuelTypeService fuelTypeService)
        {
            _fuelTypeService = fuelTypeService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _fuelTypeService.GetAllAsync();
            var model = result.Data?.Select(f => new FuelType
            {
                Id = f.Id,
                Name = f.Name,
                PricePerLiter = f.PricePerLiter,
                IsActive = f.IsActive,
                EBMSupplyPrice = f.EBMSupplyPrice,
                CreatedAt = f.CreatedAt
            }).ToList() ?? new();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name, decimal pricePerLiter, decimal? ebmSupplyPrice)
        {
            var request = new CreateFuelTypeRequestDto
            {
                Name = name,
                PricePerLiter = pricePerLiter,
                EBMSupplyPrice = ebmSupplyPrice
            };

            var result = await _fuelTypeService.CreateAsync(request);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Fuel type created successfully!" : result.Message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, string name, decimal pricePerLiter, bool isActive, decimal? ebmSupplyPrice)
        {
            var request = new UpdateFuelTypeRequestDto
            {
                Name = name,
                PricePerLiter = pricePerLiter,
                IsActive = isActive,
                EBMSupplyPrice = ebmSupplyPrice
            };

            var result = await _fuelTypeService.UpdateAsync(id, request);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Fuel type updated successfully!" : result.Message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _fuelTypeService.DeleteAsync(id);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Fuel type deleted successfully!" : result.Message;

            return RedirectToAction("Index");
        }
    }
}
