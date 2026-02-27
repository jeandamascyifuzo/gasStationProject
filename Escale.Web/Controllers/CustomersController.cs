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
                Year = car.Year
            }).ToList(),
            Subscriptions = c.Subscriptions.Select(sub => new Subscription
            {
                Id = sub.Id,
                FuelTypeName = sub.FuelTypeName,
                FuelTypeId = sub.FuelTypeId,
                MonthlyLiters = sub.MonthlyLiters,
                UsedLiters = sub.UsedLiters,
                PricePerLiter = sub.PricePerLiter,
                StartDate = sub.StartDate,
                EndDate = sub.EndDate,
                Status = sub.Status
            }).ToList()
        };
    }
}
