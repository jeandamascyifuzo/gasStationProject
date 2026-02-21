using Escale.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class TransactionsController : Controller
    {
        public IActionResult Index(TransactionFilterViewModel filter)
        {
            // TODO: Replace with actual data from database
            var model = new TransactionFilterViewModel
            {
                StationId = filter.StationId,
                StartDate = filter.StartDate ?? DateTime.Today,
                EndDate = filter.EndDate ?? DateTime.Today,
                FuelTypeId = filter.FuelTypeId,
                SearchTerm = filter.SearchTerm,
                Transactions = new List<Transaction>
                {
                    new() { Id = 1, TransactionId = "TXN001", Station = "Kigali Central", Pump = "Pump 1", FuelType = "Petrol", Quantity = 45.5m, Total = 65065, PaymentMethod = "Cash", EBMCode = "EBM001", TransactionTime = DateTime.Now.AddHours(-2), Status = "Completed" },
                    new() { Id = 2, TransactionId = "TXN002", Station = "Remera Branch", Pump = "Pump 2", FuelType = "Diesel", Quantity = 30.2m, Total = 41978, PaymentMethod = "Mobile Money", EBMCode = "EBM002", TransactionTime = DateTime.Now.AddHours(-3), Status = "Completed" },
                    new() { Id = 3, TransactionId = "TXN003", Station = "Kimironko", Pump = "Pump 1", FuelType = "Super", Quantity = 25.0m, Total = 38000, PaymentMethod = "Card", EBMCode = "EBM003", TransactionTime = DateTime.Now.AddHours(-4), Status = "Completed" }
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
                }
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Void(int id, string reason)
        {
            // TODO: Void transaction in database
            return RedirectToAction("Index");
        }
    }
}
