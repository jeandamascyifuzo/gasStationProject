using Escale.API.Domain.Constants;
using Escale.API.Domain.Entities;
using Escale.API.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Data.Seeds;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(EscaleDbContext context)
    {
        if (await context.Organizations.AnyAsync())
            return;

        var orgId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var organization = new Organization
        {
            Id = orgId,
            Name = "Escale Gas Station",
            Slug = "escale",
            TIN = "123456789",
            Address = "Kigali, Rwanda",
            Phone = "+250788000000",
            Email = "info@escale.rw",
            IsActive = true,
            CreatedAt = now
        };

        context.Organizations.Add(organization);

        // Users
        var adminId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var cashierId = Guid.NewGuid();

        var users = new List<User>
        {
            new()
            {
                Id = adminId, OrganizationId = orgId, Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FullName = "System Administrator", Role = UserRole.Admin,
                Email = "admin@escale.rw", IsActive = true, CreatedAt = now
            },
            new()
            {
                Id = managerId, OrganizationId = orgId, Username = "manager",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                FullName = "Jane Manager", Role = UserRole.Manager,
                Email = "manager@escale.rw", IsActive = true, CreatedAt = now
            },
            new()
            {
                Id = cashierId, OrganizationId = orgId, Username = "cashier",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("cashier123"),
                FullName = "John Cashier", Role = UserRole.Cashier,
                Email = "cashier@escale.rw", IsActive = true, CreatedAt = now
            }
        };
        context.Users.AddRange(users);

        // Stations
        var station1Id = Guid.NewGuid();
        var station2Id = Guid.NewGuid();

        var stations = new List<Station>
        {
            new()
            {
                Id = station1Id, OrganizationId = orgId, Name = "Main Station",
                Location = "Downtown Kigali", Address = "KN 1 Ave, Kigali",
                PhoneNumber = "+250788000001", IsActive = true, ManagerId = managerId, CreatedAt = now
            },
            new()
            {
                Id = station2Id, OrganizationId = orgId, Name = "Airport Station",
                Location = "Near Airport", Address = "KK 15 Rd, Kigali",
                PhoneNumber = "+250788000002", IsActive = true, CreatedAt = now
            }
        };
        context.Stations.AddRange(stations);

        // User-Station assignments
        context.UserStations.AddRange(
            new UserStation { UserId = adminId, StationId = station1Id, AssignedAt = now },
            new UserStation { UserId = adminId, StationId = station2Id, AssignedAt = now },
            new UserStation { UserId = managerId, StationId = station1Id, AssignedAt = now },
            new UserStation { UserId = cashierId, StationId = station1Id, AssignedAt = now }
        );

        // Fuel Types
        var fuelTypes = new List<FuelType>
        {
            new() { Id = Guid.NewGuid(), OrganizationId = orgId, Name = "Petrol 95", CurrentPrice = 1450m, IsActive = true, CreatedAt = now },
            new() { Id = Guid.NewGuid(), OrganizationId = orgId, Name = "Petrol 98", CurrentPrice = 1550m, IsActive = true, CreatedAt = now },
            new() { Id = Guid.NewGuid(), OrganizationId = orgId, Name = "Diesel", CurrentPrice = 1380m, IsActive = true, CreatedAt = now },
            new() { Id = Guid.NewGuid(), OrganizationId = orgId, Name = "Kerosene", CurrentPrice = 1200m, IsActive = true, CreatedAt = now }
        };
        context.FuelTypes.AddRange(fuelTypes);

        // Fuel Prices (initial)
        foreach (var ft in fuelTypes)
        {
            context.FuelPrices.Add(new FuelPrice
            {
                Id = Guid.NewGuid(), FuelTypeId = ft.Id, Price = ft.CurrentPrice,
                EffectiveFrom = now, CreatedAt = now
            });
        }

        // Inventory Items (per station per fuel type)
        foreach (var station in stations)
        {
            foreach (var ft in fuelTypes)
            {
                context.InventoryItems.Add(new InventoryItem
                {
                    Id = Guid.NewGuid(), OrganizationId = orgId,
                    StationId = station.Id, FuelTypeId = ft.Id,
                    CurrentLevel = 8000m, Capacity = 20000m,
                    ReorderLevel = 5000m, LastRefillDate = now,
                    CreatedAt = now
                });
            }
        }

        // Organization Settings
        context.OrganizationSettings.Add(new OrganizationSettings
        {
            Id = Guid.NewGuid(), OrganizationId = orgId,
            CompanyName = "Escale Gas Station",
            TaxRate = BusinessRules.VATRate,
            Currency = BusinessRules.DefaultCurrency,
            ReceiptHeader = BusinessRules.DefaultReceiptHeader,
            ReceiptFooter = BusinessRules.DefaultReceiptFooter,
            EBMEnabled = true,
            EBMServerUrl = "https://ebm.rra.gov.rw",
            AutoPrintReceipt = true,
            MinimumSaleAmount = BusinessRules.DefaultMinimumSaleAmount,
            MaximumSaleAmount = BusinessRules.DefaultMaximumSaleAmount,
            LowStockThreshold = 0.20m,
            CriticalStockThreshold = 0.10m,
            CreatedAt = now
        });

        await context.SaveChangesAsync();
    }
}
