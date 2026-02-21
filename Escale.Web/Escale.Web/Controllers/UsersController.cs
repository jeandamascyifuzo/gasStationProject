using Escale.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            // TODO: Replace with actual data from database
            var model = new UserViewModel
            {
                Users = new List<User>
                {
                    new() { Id = 1, Name = "Admin User", Email = "admin@escale.rw", Phone = "+250788111111", Role = "Admin", IsActive = true, LastLogin = DateTime.Now.AddHours(-1) },
                    new() { Id = 2, Name = "John Doe", Email = "john@escale.rw", Phone = "+250788123456", Role = "Manager", StationId = 1, StationName = "Kigali Central", IsActive = true, LastLogin = DateTime.Now.AddHours(-3) },
                    new() { Id = 3, Name = "Jane Smith", Email = "jane@escale.rw", Phone = "+250788234567", Role = "Manager", StationId = 2, StationName = "Remera Branch", IsActive = true, LastLogin = DateTime.Now.AddDays(-1) },
                    new() { Id = 4, Name = "Mike Cashier", Email = "mike@escale.rw", Phone = "+250788345678", Role = "Cashier", StationId = 1, StationName = "Kigali Central", IsActive = true, LastLogin = DateTime.Now.AddHours(-2) }
                },
                Stations = new List<Station>
                {
                    new() { Id = 1, Name = "Kigali Central Station" },
                    new() { Id = 2, Name = "Remera Branch" },
                    new() { Id = 3, Name = "Kimironko Station" }
                }
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(User user)
        {
            // TODO: Save to database
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(User user)
        {
            // TODO: Update in database
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ResetPassword(int id)
        {
            // TODO: Send password reset email
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            // TODO: Toggle user status in database
            return RedirectToAction("Index");
        }
    }
}
