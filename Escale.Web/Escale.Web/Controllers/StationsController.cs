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
