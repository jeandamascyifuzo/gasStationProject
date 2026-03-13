using FluentAssertions;
using Xunit;

namespace Escale.Web.Tests.Models;

/// <summary>
/// Tests the FuelType model's computed properties and all branches.
/// </summary>
public class FuelTypeModelTests
{
    [Fact]
    public void Status_ReturnsDeleted_WhenIsDeletedTrue()
    {
        var fuelType = new Escale.Web.Models.FuelType { IsDeleted = true, IsActive = true };

        fuelType.Status.Should().Be("Deleted");
    }

    [Fact]
    public void Status_ReturnsActive_WhenNotDeletedAndActive()
    {
        var fuelType = new Escale.Web.Models.FuelType { IsDeleted = false, IsActive = true };

        fuelType.Status.Should().Be("Active");
    }

    [Fact]
    public void Status_ReturnsInactive_WhenNotDeletedAndNotActive()
    {
        var fuelType = new Escale.Web.Models.FuelType { IsDeleted = false, IsActive = false };

        fuelType.Status.Should().Be("Inactive");
    }

    [Fact]
    public void AllProperties_CanBeSetAndRead()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var deletedAt = DateTime.UtcNow.AddDays(1);

        var fuelType = new Escale.Web.Models.FuelType
        {
            Id = id,
            Name = "Diesel Premium",
            PricePerLiter = 1500,
            IsActive = true,
            IsDeleted = false,
            DeletedAt = deletedAt,
            EBMProductId = "prod-1",
            EBMVariantId = "var-1",
            EBMSupplyPrice = 1200,
            IsEBMRegistered = true,
            CreatedAt = createdAt
        };

        fuelType.Id.Should().Be(id);
        fuelType.Name.Should().Be("Diesel Premium");
        fuelType.PricePerLiter.Should().Be(1500);
        fuelType.IsActive.Should().BeTrue();
        fuelType.IsDeleted.Should().BeFalse();
        fuelType.DeletedAt.Should().Be(deletedAt);
        fuelType.EBMProductId.Should().Be("prod-1");
        fuelType.EBMVariantId.Should().Be("var-1");
        fuelType.EBMSupplyPrice.Should().Be(1200);
        fuelType.IsEBMRegistered.Should().BeTrue();
        fuelType.CreatedAt.Should().Be(createdAt);
    }
}
