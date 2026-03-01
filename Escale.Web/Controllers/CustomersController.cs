using Escale.Web.Models;
using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ApiCarDto = Escale.Web.Models.Api.CarDto;

namespace Escale.Web.Controllers
{
    public class CustomersController : Controller
    {
        private readonly IApiCustomerService _customerService;

        public CustomersController(IApiCustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var result = await _customerService.GetAllAsync(page, 20, search);

            var model = new CustomerViewModel
            {
                Customers = result.Data?.Items?.Select(MapCustomer).ToList() ?? new(),
                TotalCount = result.Data?.TotalCount ?? 0,
                Page = result.Data?.Page ?? 1,
                PageSize = result.Data?.PageSize ?? 20,
                TotalPages = result.Data?.TotalPages ?? 0,
                SearchTerm = search
            };

            return View(model);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var result = await _customerService.GetByIdAsync(id);

            if (!result.Success || result.Data == null)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Index");
            }

            var c = result.Data;
            var customer = MapCustomer(c);

            var model = new CustomerDetailsViewModel
            {
                Customer = customer,
                Stats = new CustomerStats
                {
                    TotalCars = customer.Cars.Count,
                    ActiveSubscriptions = customer.Subscriptions.Count(s => s.Status == "Active"),
                    TotalSpent = 0 // Not available from API yet
                }
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Customer customer)
        {
            var request = new CreateCustomerRequestDto
            {
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email,
                Type = customer.Type,
                TIN = customer.TIN,
                CreditLimit = customer.CreditLimit,
                Cars = customer.Cars?.Select(c => new ApiCarDto
                {
                    PlateNumber = c.PlateNumber,
                    Make = c.Make,
                    Model = c.Model,
                    Year = c.Year
                }).ToList()
            };

            var result = await _customerService.CreateAsync(request);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Customer created successfully!" : result.Message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Customer customer)
        {
            var request = new UpdateCustomerRequestDto
            {
                Name = customer.Name,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email,
                Type = customer.Type,
                TIN = customer.TIN,
                CreditLimit = customer.CreditLimit,
                IsActive = customer.IsActive
            };

            var result = await _customerService.UpdateAsync(customer.Id, request);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Customer updated successfully!" : result.Message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _customerService.DeleteAsync(id);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Customer deleted successfully!" : result.Message;

            return RedirectToAction("Index");
        }

        // Car CRUD
        [HttpPost]
        public async Task<IActionResult> AddCar(Guid customerId, string plateNumber, string? make, string? model, int? year, string pin)
        {
            var car = new ApiCarDto
            {
                PlateNumber = plateNumber,
                Make = make,
                Model = model,
                Year = year,
                PIN = pin
            };

            var result = await _customerService.AddCarAsync(customerId, car);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Car added successfully!" : result.Message;

            return RedirectToAction("Details", new { id = customerId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCar(Guid customerId, Guid carId, string plateNumber, string? make, string? model, int? year, string? pin)
        {
            var car = new ApiCarDto
            {
                Id = carId,
                PlateNumber = plateNumber,
                Make = make,
                Model = model,
                Year = year,
                PIN = pin
            };

            var result = await _customerService.UpdateCarAsync(customerId, carId, car);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Car updated successfully!" : result.Message;

            return RedirectToAction("Details", new { id = customerId });
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateCar(Guid customerId, Guid carId)
        {
            var result = await _customerService.DeactivateCarAsync(customerId, carId);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Car deactivated successfully!" : result.Message;

            return RedirectToAction("Details", new { id = customerId });
        }

        // Subscriptions
        [HttpPost]
        public async Task<IActionResult> TopUpSubscription(Guid customerId, decimal topUpAmount, DateTime? expiryDate)
        {
            var request = new TopUpSubscriptionRequestDto
            {
                CustomerId = customerId,
                TopUpAmount = topUpAmount,
                ExpiryDate = expiryDate
            };

            var result = await _customerService.TopUpSubscriptionAsync(request);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? $"Subscription topped up with {topUpAmount:N0} RWF!" : result.Message;

            return RedirectToAction("Details", new { id = customerId });
        }

        [HttpPost]
        public async Task<IActionResult> CancelSubscription(Guid customerId, Guid subscriptionId)
        {
            var result = await _customerService.CancelSubscriptionAsync(subscriptionId);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success ? "Subscription cancelled successfully!" : result.Message;

            return RedirectToAction("Details", new { id = customerId });
        }

        private static Customer MapCustomer(CustomerResponseDto c) => new()
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email,
            PhoneNumber = c.PhoneNumber,
            Type = c.Type,
            TIN = c.TIN,
            CreditLimit = c.CreditLimit,
            CurrentCredit = c.CurrentCredit,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            Cars = c.Cars.Select(car => new Car
            {
                Id = car.Id,
                PlateNumber = car.PlateNumber,
                Make = car.Make,
                Model = car.Model,
                Year = car.Year,
                IsActive = car.IsActive
            }).ToList(),
            Subscriptions = c.Subscriptions.Select(sub => new Subscription
            {
                Id = sub.Id,
                TotalAmount = sub.TotalAmount,
                RemainingBalance = sub.RemainingBalance,
                PreviousBalance = sub.PreviousBalance,
                TopUpAmount = sub.TopUpAmount,
                StartDate = sub.StartDate,
                ExpiryDate = sub.ExpiryDate,
                Status = sub.Status,
                CreatedAt = sub.CreatedAt
            }).ToList()
        };
    }
}
