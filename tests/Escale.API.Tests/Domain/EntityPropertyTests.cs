using Escale.API.Domain.Entities;
using Escale.API.DTOs.EBM;
using Escale.API.DTOs.FuelTypes;
using FluentAssertions;
using Xunit;

namespace Escale.API.Tests.Domain;

/// <summary>
/// Ensures all entity and DTO properties are exercised for full coverage.
/// </summary>
public class EntityPropertyTests
{
    [Fact]
    public void BaseEntity_AllProperties()
    {
        var now = DateTime.UtcNow;
        var id = Guid.NewGuid();

        // Use FuelPrice as a concrete BaseEntity (not TenantEntity)
        var entity = new FuelPrice
        {
            Id = id,
            CreatedAt = now,
            UpdatedAt = now.AddHours(1),
            CreatedBy = "admin",
            UpdatedBy = "manager",
            IsDeleted = true,
            DeletedAt = now.AddDays(1),
            FuelTypeId = Guid.NewGuid(),
            FuelType = new FuelType(),
            Price = 1200,
            EffectiveFrom = now,
            EffectiveTo = now.AddDays(30)
        };

        entity.Id.Should().Be(id);
        entity.CreatedAt.Should().Be(now);
        entity.UpdatedAt.Should().Be(now.AddHours(1));
        entity.CreatedBy.Should().Be("admin");
        entity.UpdatedBy.Should().Be("manager");
        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAt.Should().Be(now.AddDays(1));
        entity.FuelTypeId.Should().NotBeEmpty();
        entity.FuelType.Should().NotBeNull();
        entity.Price.Should().Be(1200);
        entity.EffectiveFrom.Should().Be(now);
        entity.EffectiveTo.Should().Be(now.AddDays(30));
    }

    [Fact]
    public void FuelType_AllProperties()
    {
        var fuelType = new FuelType
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Organization = new Organization(),
            Name = "Diesel",
            CurrentPrice = 1200,
            IsActive = true,
            EBMProductId = "prod-1",
            EBMVariantId = "var-1",
            EBMSupplyPrice = 1000,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "admin",
            UpdatedBy = "admin",
            IsDeleted = false,
            DeletedAt = null
        };

        fuelType.Name.Should().Be("Diesel");
        fuelType.CurrentPrice.Should().Be(1200);
        fuelType.IsActive.Should().BeTrue();
        fuelType.EBMProductId.Should().Be("prod-1");
        fuelType.EBMVariantId.Should().Be("var-1");
        fuelType.EBMSupplyPrice.Should().Be(1000);
        fuelType.OrganizationId.Should().NotBeEmpty();
        fuelType.Organization.Should().NotBeNull();
        fuelType.PriceHistory.Should().NotBeNull();
        fuelType.InventoryItems.Should().NotBeNull();
        fuelType.Transactions.Should().NotBeNull();
    }

    [Fact]
    public void Station_AllProperties()
    {
        var station = new Station
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Organization = new Organization(),
            Name = "Main Station",
            Location = "Kigali",
            Address = "123 Main St",
            PhoneNumber = "+250788000000",
            IsActive = true,
            ManagerId = Guid.NewGuid(),
            Manager = new User(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "admin",
            UpdatedBy = "admin",
            IsDeleted = false,
            DeletedAt = null
        };

        station.Name.Should().Be("Main Station");
        station.Location.Should().Be("Kigali");
        station.Address.Should().Be("123 Main St");
        station.PhoneNumber.Should().Be("+250788000000");
        station.IsActive.Should().BeTrue();
        station.ManagerId.Should().NotBeNull();
        station.Manager.Should().NotBeNull();
        station.UserStations.Should().NotBeNull();
        station.Transactions.Should().NotBeNull();
        station.InventoryItems.Should().NotBeNull();
        station.Shifts.Should().NotBeNull();
    }

    [Fact]
    public void InventoryItem_AllProperties()
    {
        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Organization = new Organization(),
            StationId = Guid.NewGuid(),
            Station = new Station(),
            FuelTypeId = Guid.NewGuid(),
            FuelType = new FuelType(),
            CurrentLevel = 15000,
            Capacity = 20000,
            ReorderLevel = 5000,
            LastRefillDate = DateTime.UtcNow.AddDays(-5),
            NextDeliveryDate = DateTime.UtcNow.AddDays(2),
            EBMStockId = "stk-1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            UpdatedBy = "system",
            IsDeleted = false,
            DeletedAt = null
        };

        item.StationId.Should().NotBeEmpty();
        item.Station.Should().NotBeNull();
        item.FuelTypeId.Should().NotBeEmpty();
        item.FuelType.Should().NotBeNull();
        item.CurrentLevel.Should().Be(15000);
        item.Capacity.Should().Be(20000);
        item.ReorderLevel.Should().Be(5000);
        item.LastRefillDate.Should().NotBeNull();
        item.NextDeliveryDate.Should().NotBeNull();
        item.EBMStockId.Should().Be("stk-1");
        item.RefillRecords.Should().NotBeNull();
    }

    [Fact]
    public void OrganizationSettings_AllProperties()
    {
        var settings = new OrganizationSettings
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Organization = new Organization(),
            CompanyName = "Escale Inc",
            TaxRate = 0.18m,
            Currency = "RWF",
            ReceiptHeader = "Header",
            ReceiptFooter = "Footer",
            EBMEnabled = true,
            EBMServerUrl = "https://ebm.example.com",
            EBMBusinessId = "biz-1",
            EBMBranchId = "branch-1",
            EBMCompanyName = "Escale",
            EBMCompanyAddress = "Kigali",
            EBMCompanyPhone = "+250788",
            EBMCompanyTIN = "123456789",
            EBMCategoryId = "cat-1",
            AutoPrintReceipt = true,
            RequireCustomerInfo = false,
            MinimumSaleAmount = 1000,
            MaximumSaleAmount = 10_000_000,
            AllowNegativeStock = false,
            LowStockThreshold = 0.20m,
            CriticalStockThreshold = 0.10m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "admin",
            UpdatedBy = "admin",
            IsDeleted = false,
            DeletedAt = null
        };

        settings.CompanyName.Should().Be("Escale Inc");
        settings.TaxRate.Should().Be(0.18m);
        settings.Currency.Should().Be("RWF");
        settings.ReceiptHeader.Should().Be("Header");
        settings.ReceiptFooter.Should().Be("Footer");
        settings.EBMEnabled.Should().BeTrue();
        settings.EBMServerUrl.Should().NotBeNull();
        settings.EBMBusinessId.Should().NotBeNull();
        settings.EBMBranchId.Should().NotBeNull();
        settings.EBMCompanyName.Should().NotBeNull();
        settings.EBMCompanyAddress.Should().NotBeNull();
        settings.EBMCompanyPhone.Should().NotBeNull();
        settings.EBMCompanyTIN.Should().NotBeNull();
        settings.EBMCategoryId.Should().NotBeNull();
        settings.AutoPrintReceipt.Should().BeTrue();
        settings.RequireCustomerInfo.Should().BeFalse();
        settings.MinimumSaleAmount.Should().Be(1000);
        settings.MaximumSaleAmount.Should().Be(10_000_000);
        settings.AllowNegativeStock.Should().BeFalse();
        settings.LowStockThreshold.Should().Be(0.20m);
        settings.CriticalStockThreshold.Should().Be(0.10m);
    }

    [Fact]
    public void EBMCreateProductResult_AllProperties()
    {
        var result = new EBMCreateProductResult
        {
            Success = true,
            ProductId = "prod-1",
            VariantId = "var-1",
            StockId = "stk-1",
            ErrorMessage = "none",
            RawResponse = "{}"
        };

        result.Success.Should().BeTrue();
        result.ProductId.Should().Be("prod-1");
        result.VariantId.Should().Be("var-1");
        result.StockId.Should().Be("stk-1");
        result.ErrorMessage.Should().Be("none");
        result.RawResponse.Should().Be("{}");
    }

    [Fact]
    public void FuelTypeResponseDto_AllProperties()
    {
        var dto = new FuelTypeResponseDto
        {
            Id = Guid.NewGuid(),
            Name = "Diesel",
            PricePerLiter = 1200,
            IsActive = true,
            IsDeleted = false,
            DeletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            EBMProductId = "prod-1",
            EBMVariantId = "var-1",
            EBMSupplyPrice = 1000,
            EBMRegistered = true
        };

        dto.Id.Should().NotBeEmpty();
        dto.Name.Should().Be("Diesel");
        dto.PricePerLiter.Should().Be(1200);
        dto.IsActive.Should().BeTrue();
        dto.IsDeleted.Should().BeFalse();
        dto.DeletedAt.Should().NotBeNull();
        dto.CreatedAt.Should().NotBe(default);
        dto.EBMProductId.Should().Be("prod-1");
        dto.EBMVariantId.Should().Be("var-1");
        dto.EBMSupplyPrice.Should().Be(1000);
        dto.EBMRegistered.Should().BeTrue();
    }

    [Fact]
    public void CreateFuelTypeRequestDto_AllProperties()
    {
        var dto = new CreateFuelTypeRequestDto
        {
            Name = "Diesel",
            PricePerLiter = 1200,
            EBMVariantId = "var-1",
            EBMSupplyPrice = 1000
        };

        dto.Name.Should().Be("Diesel");
        dto.PricePerLiter.Should().Be(1200);
        dto.EBMVariantId.Should().Be("var-1");
        dto.EBMSupplyPrice.Should().Be(1000);
    }

    [Fact]
    public void UpdateFuelTypeRequestDto_AllProperties()
    {
        var dto = new UpdateFuelTypeRequestDto
        {
            Name = "Diesel Pro",
            PricePerLiter = 1500,
            IsActive = false,
            EBMVariantId = "var-2",
            EBMSupplyPrice = 1200
        };

        dto.Name.Should().Be("Diesel Pro");
        dto.PricePerLiter.Should().Be(1500);
        dto.IsActive.Should().BeFalse();
        dto.EBMVariantId.Should().Be("var-2");
        dto.EBMSupplyPrice.Should().Be(1200);
    }
}
