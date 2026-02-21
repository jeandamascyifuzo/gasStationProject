using Escale.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class FuelTypesController : Controller
    {
        public IActionResult Index()
        {
            // TODO: Replace with actual data from database
            var model = new List<FuelType>
            {
                new() { Id = 1, Name = "Petrol", Code = "PET", SellingPrice = 1430, CostPrice = 1350, VATRate = 18, IsActive = true },
                new() { Id = 2, Name = "Diesel", Code = "DSL", SellingPrice = 1390, CostPrice = 1320, VATRate = 18, IsActive = true },
                new() { Id = 3, Name = "Super", Code = "SUP", SellingPrice = 1520, CostPrice = 1440, VATRate = 18, IsActive = true }
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(FuelType fuelType)
        {
            // TODO: Save to database
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(FuelType fuelType)
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
