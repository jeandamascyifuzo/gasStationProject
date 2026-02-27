using Escale.API.Domain.Constants;
using Escale.API.Domain.Entities;
using Escale.API.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Data.Seeds;

public static class DatabaseSeeder
{
    private static readonly Random _random = new(42); // Fixed seed for deterministic data

    public static async Task SeedAsync(EscaleDbContext context)
    {
        if (await context.Organizations.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        // ── System Organization (for SuperAdmin) ──
        var systemOrgId = Guid.NewGuid();
        context.Organizations.Add(new Organization
        {
            Id = systemOrgId,
            Name = "Escale System",
            Slug = "system",
            TIN = "000000000",
            Address = "System",
            IsActive = true,
            CreatedAt = now
        });

        context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = systemOrgId,
            Username = "superadmin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("superadmin123"),
            FullName = "Super Administrator",
            Email = "superadmin@escale.rw",
            Role = UserRole.SuperAdmin,
            IsActive = true,
            CreatedAt = now
        });

        context.OrganizationSettings.Add(new OrganizationSettings
        {
            Id = Guid.NewGuid(),
            OrganizationId = systemOrgId,
            CompanyName = "Escale System",
            TaxRate = BusinessRules.VATRate,
            Currency = BusinessRules.DefaultCurrency,
            ReceiptHeader = "ESCALE SYSTEM",
            ReceiptFooter = BusinessRules.DefaultReceiptFooter,
            MinimumSaleAmount = BusinessRules.DefaultMinimumSaleAmount,
            MaximumSaleAmount = BusinessRules.DefaultMaximumSaleAmount,
            LowStockThreshold = 0.20m,
            CriticalStockThreshold = 0.10m,
            CreatedAt = now
        });

        // ── Organization 1: Escale Petroleum ──
        await SeedOrganization(context, now, new OrgSeedData
        {
            Name = "Escale Petroleum",
            Slug = "escale-petroleum",
            TIN = "111222333",
            Address = "KN 1 Ave, Kigali, Rwanda",
            Phone = "+250788100000",
            Email = "info@escale-petroleum.rw",
            EBMEnabled = true,
            EBMServerUrl = "https://ebm.rra.gov.rw",
            Users = new[]
            {
                ("admin", "admin123", "Jean Pierre Admin", "admin@escale-petroleum.rw", "+250788100001", UserRole.Admin),
                ("manager", "manager123", "Marie Claire Manager", "manager@escale-petroleum.rw", "+250788100002", UserRole.Manager),
                ("cashier", "cashier123", "Patrick Cashier", "cashier@escale-petroleum.rw", "+250788100003", UserRole.Cashier),
            },
            Stations = new[]
            {
                ("Kigali Main Station", "Downtown Kigali", "KN 1 Ave, Kigali", "+250788110001"),
                ("Kimironko Station", "Kimironko", "KG 11 Ave, Kigali", "+250788110002"),
            },
            FuelTypes = new[]
            {
                ("Petrol 95", 1450m),
                ("Petrol 98", 1550m),
                ("Diesel", 1380m),
                ("Kerosene", 1200m),
            },
            InventoryLevel = 8000m,
            InventoryCapacity = 20000m,
            Customers = new[]
            {
                ("Uwimana Jean", "+250788200001", "uwimana@gmail.com", CustomerType.Individual, "RAD 123A"),
                ("Mukamana Grace", "+250788200002", "mukamana@gmail.com", CustomerType.Individual, "RAE 456B"),
                ("Habimana Eric", "+250788200003", "habimana@gmail.com", CustomerType.Individual, "RAF 789C"),
                ("Total Transport Ltd", "+250788200004", "info@totaltransport.rw", CustomerType.Corporate, "RAG 012D"),
                ("Kigali Bus Services", "+250788200005", "fleet@kbs.rw", CustomerType.Corporate, "RAH 345E"),
            },
            TransactionCount = 20,
        });

        // ── Organization 2: Green Energy Fuels ──
        await SeedOrganization(context, now, new OrgSeedData
        {
            Name = "Green Energy Fuels",
            Slug = "green-energy",
            TIN = "444555666",
            Address = "Musanze, Rwanda",
            Phone = "+250788300000",
            Email = "info@greenenergy.rw",
            EBMEnabled = false,
            Users = new[]
            {
                ("admin2", "admin123", "Emmanuel Admin", "admin@greenenergy.rw", "+250788300001", UserRole.Admin),
                ("manager2", "manager123", "Diane Manager", "manager@greenenergy.rw", "+250788300002", UserRole.Manager),
                ("cashier2", "cashier123", "Claude Cashier", "cashier@greenenergy.rw", "+250788300003", UserRole.Cashier),
            },
            Stations = new[]
            {
                ("Musanze Central", "Musanze Town", "NR 4, Musanze", "+250788310001"),
                ("Rubavu Branch", "Rubavu", "Gisenyi Rd, Rubavu", "+250788310002"),
            },
            FuelTypes = new[]
            {
                ("Petrol 95", 1480m),
                ("Diesel", 1400m),
                ("LPG", 850m),
            },
            InventoryLevel = 6000m,
            InventoryCapacity = 15000m,
            Customers = new[]
            {
                ("Niyonzima Paul", "+250788400001", "paul@gmail.com", CustomerType.Individual, "RAJ 111A"),
                ("Musanze Tours", "+250788400002", "info@musanzetours.rw", CustomerType.Corporate, "RAK 222B"),
                ("Ingabo Michel", "+250788400003", "michel@gmail.com", CustomerType.Individual, "RAL 333C"),
            },
            TransactionCount = 15,
        });

        // ── Organization 3: Rwanda Fuel Corp ──
        await SeedOrganization(context, now, new OrgSeedData
        {
            Name = "Rwanda Fuel Corp",
            Slug = "rwanda-fuel",
            TIN = "777888999",
            Address = "Huye, Rwanda",
            Phone = "+250788500000",
            Email = "info@rwandafuel.rw",
            EBMEnabled = true,
            EBMServerUrl = "https://ebm-test.rra.gov.rw",
            Users = new[]
            {
                ("admin3", "admin123", "Alice Admin", "admin@rwandafuel.rw", "+250788500001", UserRole.Admin),
                ("cashier3", "cashier123", "Bob Cashier", "cashier@rwandafuel.rw", "+250788500002", UserRole.Cashier),
            },
            Stations = new[]
            {
                ("Huye Station", "Huye Town", "NR 1, Huye", "+250788510001"),
            },
            FuelTypes = new[]
            {
                ("Petrol 95", 1460m),
                ("Diesel", 1390m),
            },
            InventoryLevel = 10000m,
            InventoryCapacity = 25000m,
            Customers = Array.Empty<(string, string, string, CustomerType, string)>(),
            TransactionCount = 10,
        });

        await context.SaveChangesAsync();
    }

    private static Task SeedOrganization(EscaleDbContext context, DateTime now, OrgSeedData data)
    {
        var orgId = Guid.NewGuid();

        // Organization
        context.Organizations.Add(new Organization
        {
            Id = orgId,
            Name = data.Name,
            Slug = data.Slug,
            TIN = data.TIN,
            Address = data.Address,
            Phone = data.Phone,
            Email = data.Email,
            IsActive = true,
            CreatedAt = now
        });

        // Users
        var userIds = new List<(Guid Id, UserRole Role)>();
        Guid? managerId = null;
        Guid? firstAdminId = null;
        Guid? firstCashierId = null;

        foreach (var (username, password, fullName, email, phone, role) in data.Users)
        {
            var userId = Guid.NewGuid();
            context.Users.Add(new User
            {
                Id = userId,
                OrganizationId = orgId,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FullName = fullName,
                Email = email,
                Phone = phone,
                Role = role,
                IsActive = true,
                CreatedAt = now
            });
            userIds.Add((userId, role));

            if (role == UserRole.Manager && managerId == null) managerId = userId;
            if (role == UserRole.Admin && firstAdminId == null) firstAdminId = userId;
            if (role == UserRole.Cashier && firstCashierId == null) firstCashierId = userId;
        }

        // Stations
        var stationIds = new List<Guid>();
        var isFirst = true;
        foreach (var (name, location, address, phone) in data.Stations)
        {
            var stationId = Guid.NewGuid();
            context.Stations.Add(new Station
            {
                Id = stationId,
                OrganizationId = orgId,
                Name = name,
                Location = location,
                Address = address,
                PhoneNumber = phone,
                IsActive = true,
                ManagerId = isFirst ? managerId : null,
                CreatedAt = now
            });
            stationIds.Add(stationId);
            isFirst = false;
        }

        // User-Station assignments
        foreach (var (userId, role) in userIds)
        {
            if (role == UserRole.Admin || role == UserRole.Manager)
            {
                // Admin and Manager assigned to all stations
                foreach (var stationId in stationIds)
                    context.UserStations.Add(new UserStation { UserId = userId, StationId = stationId, AssignedAt = now });
            }
            else if (stationIds.Count > 0)
            {
                // Cashier assigned to first station only
                context.UserStations.Add(new UserStation { UserId = userId, StationId = stationIds[0], AssignedAt = now });
            }
        }

        // Fuel Types
        var fuelTypes = new List<FuelType>();
        foreach (var (name, price) in data.FuelTypes)
        {
            var ft = new FuelType
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId,
                Name = name,
                CurrentPrice = price,
                IsActive = true,
                CreatedAt = now
            };
            context.FuelTypes.Add(ft);
            fuelTypes.Add(ft);

            context.FuelPrices.Add(new FuelPrice
            {
                Id = Guid.NewGuid(),
                FuelTypeId = ft.Id,
                Price = price,
                EffectiveFrom = now.AddDays(-30),
                CreatedAt = now
            });
        }

        // Inventory Items (per station per fuel type)
        foreach (var stationId in stationIds)
        {
            foreach (var ft in fuelTypes)
            {
                context.InventoryItems.Add(new InventoryItem
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = orgId,
                    StationId = stationId,
                    FuelTypeId = ft.Id,
                    CurrentLevel = data.InventoryLevel,
                    Capacity = data.InventoryCapacity,
                    ReorderLevel = data.InventoryCapacity * 0.25m,
                    LastRefillDate = now.AddDays(-3),
                    CreatedAt = now
                });
            }
        }

        // Customers
        var customerIds = new List<Guid>();
        foreach (var (name, phone, email, type, plate) in data.Customers)
        {
            var customerId = Guid.NewGuid();
            var customer = new Customer
            {
                Id = customerId,
                OrganizationId = orgId,
                Name = name,
                PhoneNumber = phone,
                Email = email,
                Type = type,
                CreditLimit = type == CustomerType.Corporate ? 500000m : 100000m,
                CurrentCredit = 0m,
                IsActive = true,
                CreatedAt = now
            };
            context.Customers.Add(customer);
            customerIds.Add(customerId);

            context.Cars.Add(new Car
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                PlateNumber = plate,
                Make = GetRandomCarMake(),
                Model = "Sedan",
                Year = _random.Next(2018, 2025),
                CreatedAt = now
            });
        }

        // Transactions
        if (data.TransactionCount > 0 && stationIds.Count > 0 && fuelTypes.Count > 0)
        {
            var cashierId = firstCashierId ?? firstAdminId ?? userIds[0].Id;
            SeedTransactions(context, orgId, stationIds, fuelTypes, customerIds, cashierId, data.TransactionCount, now);
        }

        // Organization Settings
        context.OrganizationSettings.Add(new OrganizationSettings
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            CompanyName = data.Name,
            TaxRate = BusinessRules.VATRate,
            Currency = BusinessRules.DefaultCurrency,
            ReceiptHeader = $"{data.Name}\n{data.Address}",
            ReceiptFooter = BusinessRules.DefaultReceiptFooter,
            EBMEnabled = data.EBMEnabled,
            EBMServerUrl = data.EBMServerUrl,
            AutoPrintReceipt = true,
            MinimumSaleAmount = BusinessRules.DefaultMinimumSaleAmount,
            MaximumSaleAmount = BusinessRules.DefaultMaximumSaleAmount,
            LowStockThreshold = 0.20m,
            CriticalStockThreshold = 0.10m,
            CreatedAt = now
        });

        return Task.CompletedTask;
    }

    private static void SeedTransactions(
        EscaleDbContext context, Guid orgId,
        List<Guid> stationIds, List<FuelType> fuelTypes,
        List<Guid> customerIds, Guid cashierId,
        int count, DateTime now)
    {
        var paymentMethods = Enum.GetValues<PaymentMethod>();

        for (int i = 0; i < count; i++)
        {
            var daysAgo = _random.Next(0, 30);
            var hoursAgo = _random.Next(6, 20);
            var txDate = now.AddDays(-daysAgo).Date.AddHours(hoursAgo).AddMinutes(_random.Next(0, 60));

            var stationId = stationIds[_random.Next(stationIds.Count)];
            var ft = fuelTypes[_random.Next(fuelTypes.Count)];
            var liters = Math.Round((decimal)(_random.Next(5, 80) + _random.NextDouble()), 2);
            var pricePerLiter = ft.CurrentPrice;
            var subtotal = Math.Round(liters * pricePerLiter, 2);
            var vat = Math.Round(subtotal * BusinessRules.VATRate, 2);
            var total = subtotal + vat;
            var payment = paymentMethods[_random.Next(paymentMethods.Length)];

            Guid? customerId = null;
            string? customerName = null;
            string? customerPhone = null;
            if (customerIds.Count > 0 && _random.Next(3) == 0) // ~33% have customer
            {
                customerId = customerIds[_random.Next(customerIds.Count)];
            }

            var receiptNumber = $"RCP{txDate:yyyyMMddHHmmss}{i:D3}";

            context.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                OrganizationId = orgId,
                ReceiptNumber = receiptNumber,
                TransactionDate = txDate,
                StationId = stationId,
                FuelTypeId = ft.Id,
                Liters = liters,
                PricePerLiter = pricePerLiter,
                Subtotal = subtotal,
                VATAmount = vat,
                Total = total,
                PaymentMethod = payment,
                Status = TransactionStatus.Completed,
                CustomerId = customerId,
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                CashierId = cashierId,
                EBMSent = _random.Next(2) == 0,
                CreatedAt = txDate
            });
        }
    }

    private static string GetRandomCarMake()
    {
        var makes = new[] { "Toyota", "Honda", "Nissan", "Hyundai", "Suzuki", "Mitsubishi", "Volkswagen", "Mercedes" };
        return makes[_random.Next(makes.Length)];
    }

    private class OrgSeedData
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string TIN { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool EBMEnabled { get; set; }
        public string? EBMServerUrl { get; set; }
        public (string Username, string Password, string FullName, string Email, string Phone, UserRole Role)[] Users { get; set; } = Array.Empty<(string, string, string, string, string, UserRole)>();
        public (string Name, string Location, string Address, string Phone)[] Stations { get; set; } = Array.Empty<(string, string, string, string)>();
        public (string Name, decimal Price)[] FuelTypes { get; set; } = Array.Empty<(string, decimal)>();
        public decimal InventoryLevel { get; set; }
        public decimal InventoryCapacity { get; set; }
        public (string Name, string Phone, string Email, CustomerType Type, string Plate)[] Customers { get; set; } = Array.Empty<(string, string, string, CustomerType, string)>();
        public int TransactionCount { get; set; }
    }
}
