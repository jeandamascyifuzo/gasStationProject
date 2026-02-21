using Escale.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Escale.Web.Controllers
{
    public class CustomersController : Controller
    {
        public IActionResult Index()
        {
            // TODO: Replace with actual data from database
            var model = new CustomerViewModel
            {
                Customers = new List<Customer>
                {
                    new() 
                    { 
                        Id = 1, 
                        Name = "Jean Dupont", 
                        Email = "jean.dupont@email.com", 
                        Phone = "+250788111111", 
                        CustomerType = "Individual",
                        IsActive = true,
                        CreatedAt = DateTime.Now.AddMonths(-6),
                        Cars = new List<Car> 
                        { 
                            new() { PlateNumber = "RAA 123 B", Model = "Toyota Camry" },
                            new() { PlateNumber = "RAB 456 C", Model = "Honda Accord" }
                        },
                        Subscriptions = new List<Subscription>
                        {
                            new() { SubscriptionType = "Premium", Amount = 50000, IsActive = true }
                        }
                    },
                    new() 
                    { 
                        Id = 2, 
                        Name = "Kwizera Limited", 
                        Email = "info@kwizera.rw", 
                        Phone = "+250788222222", 
                        CustomerType = "Corporate",
                        CompanyName = "Kwizera Limited",
                        TIN = "123456789",
                        IsActive = true,
                        CreatedAt = DateTime.Now.AddMonths(-12),
                        Cars = new List<Car> 
                        { 
                            new() { PlateNumber = "RAC 789 D", Model = "Toyota Hilux" }
                        },
                        Subscriptions = new List<Subscription>
                        {
                            new() { SubscriptionType = "Corporate", Amount = 200000, IsActive = true }
                        }
                    },
                    new() 
                    { 
                        Id = 3, 
                        Name = "Marie Uwase", 
                        Email = "marie.uwase@email.com", 
                        Phone = "+250788333333", 
                        CustomerType = "Individual",
                        IsActive = true,
                        CreatedAt = DateTime.Now.AddMonths(-3),
                        Cars = new List<Car> 
                        { 
                            new() { PlateNumber = "RAD 321 E", Model = "Nissan X-Trail" }
                        },
                        Subscriptions = new List<Subscription>()
                    }
                }
            };

            return View(model);
        }

        public IActionResult Details(int id)
        {
            // TODO: Replace with actual data from database
            var customer = new Customer
            {
                Id = id,
                Name = "Jean Dupont",
                Email = "jean.dupont@email.com",
                Phone = "+250788111111",
                Address = "KN 4 Ave, Kigali",
                CustomerType = "Individual",
                IsActive = true,
                CreatedAt = DateTime.Now.AddMonths(-6),
                UpdatedAt = DateTime.Now.AddDays(-5)
            };

            var cars = new List<Car>
            {
                new() 
                { 
                    Id = 1,
                    CustomerId = id,
                    PlateNumber = "RAA 123 B", 
                    CarPin = "1234",
                    Model = "Camry",
                    Make = "Toyota",
                    Year = 2020,
                    Color = "Silver",
                    CreatedAt = DateTime.Now.AddMonths(-6),
                    UpdatedAt = DateTime.Now.AddMonths(-2)
                },
                new() 
                { 
                    Id = 2,
                    CustomerId = id,
                    PlateNumber = "RAB 456 C", 
                    CarPin = "5678",
                    Model = "Accord",
                    Make = "Honda",
                    Year = 2019,
                    Color = "Black",
                    CreatedAt = DateTime.Now.AddMonths(-4),
                    UpdatedAt = DateTime.Now.AddMonths(-1)
                }
            };

            var subscriptions = new List<Subscription>
            {
                new() 
                { 
                    Id = 1,
                    CustomerId = id,
                    SubscriptionType = "Premium Monthly", 
                    Amount = 50000, 
                    IsActive = true,
                    ActivatedDate = DateTime.Now.AddMonths(-3),
                    ExpiryDate = DateTime.Now.AddMonths(9),
                    FuelType = "Petrol",
                    MonthlyLimit = 100000,
                    CurrentUsage = 35000,
                    PaymentFrequency = "Monthly",
                    CreatedAt = DateTime.Now.AddMonths(-3)
                },
                new() 
                { 
                    Id = 2,
                    CustomerId = id,
                    SubscriptionType = "Diesel Package", 
                    Amount = 80000, 
                    IsActive = false,
                    ActivatedDate = DateTime.Now.AddMonths(-12),
                    ExpiryDate = DateTime.Now.AddMonths(-6),
                    FuelType = "Diesel",
                    MonthlyLimit = 150000,
                    CurrentUsage = 0,
                    PaymentFrequency = "Monthly",
                    CreatedAt = DateTime.Now.AddMonths(-12),
                    UpdatedAt = DateTime.Now.AddMonths(-6)
                }
            };

            var stats = new CustomerStats
            {
                TotalCars = 2,
                ActiveSubscriptions = 1,
                TotalSpent = 450000,
                LastTransaction = DateTime.Now.AddDays(-2)
            };

            var model = new CustomerDetailsViewModel
            {
                Customer = customer,
                Cars = cars,
                Subscriptions = subscriptions,
                Stats = stats
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(Customer customer)
        {
            // TODO: Save to database
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(Customer customer)
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

        [HttpPost]
        public IActionResult AddCar(Car car)
        {
            // TODO: Save car to database
            return RedirectToAction("Details", new { id = car.CustomerId });
        }

        [HttpPost]
        public IActionResult EditCar(Car car)
        {
            // TODO: Update car in database
            return RedirectToAction("Details", new { id = car.CustomerId });
        }

        [HttpPost]
        public IActionResult DeleteCar(int id, int customerId)
        {
            // TODO: Delete car from database
            return RedirectToAction("Details", new { id = customerId });
        }

        [HttpPost]
        public IActionResult AddSubscription(Subscription subscription)
        {
            // TODO: Save subscription to database
            return RedirectToAction("Details", new { id = subscription.CustomerId });
        }

        [HttpPost]
        public IActionResult EditSubscription(Subscription subscription)
        {
            // TODO: Update subscription in database
            return RedirectToAction("Details", new { id = subscription.CustomerId });
        }

        [HttpPost]
        public IActionResult CancelSubscription(int id, int customerId)
        {
            // TODO: Cancel subscription in database
            return RedirectToAction("Details", new { id = customerId });
        }
    }
}
