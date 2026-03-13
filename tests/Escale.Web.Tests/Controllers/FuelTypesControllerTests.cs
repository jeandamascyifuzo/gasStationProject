using Escale.Web.Controllers;
using Escale.Web.Models.Api;
using Escale.Web.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace Escale.Web.Tests.Controllers;

public class FuelTypesControllerTests
{
    private readonly Mock<IApiFuelTypeService> _fuelTypeServiceMock;
    private readonly FuelTypesController _controller;

    public FuelTypesControllerTests()
    {
        _fuelTypeServiceMock = new Mock<IApiFuelTypeService>();
        _controller = new FuelTypesController(_fuelTypeServiceMock.Object);

        // Set up TempData (required for redirect messages)
        _controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>());
    }

    #region Index

    [Fact]
    public async Task Index_ReturnsViewResult_WithFuelTypeList()
    {
        // Arrange
        var apiResponse = new ApiResponse<List<FuelTypeResponseDto>>
        {
            Success = true,
            Data = new List<FuelTypeResponseDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Diesel",
                    PricePerLiter = 1200,
                    IsActive = true,
                    EBMRegistered = true,
                    EBMProductId = "prod-1",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Petrol",
                    PricePerLiter = 1350,
                    IsActive = true,
                    EBMRegistered = false,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };
        _fuelTypeServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<List<Escale.Web.Models.FuelType>>().Subject;
        model.Should().HaveCount(2);
        model[0].Name.Should().Be("Diesel");
        model[0].IsEBMRegistered.Should().BeTrue();
        model[1].Name.Should().Be("Petrol");
        model[1].IsEBMRegistered.Should().BeFalse();
    }

    [Fact]
    public async Task Index_ReturnsEmptyList_WhenDataIsNull()
    {
        // Arrange
        var apiResponse = new ApiResponse<List<FuelTypeResponseDto>>
        {
            Success = false,
            Data = null
        };
        _fuelTypeServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<List<Escale.Web.Models.FuelType>>().Subject;
        model.Should().BeEmpty();
    }

    [Fact]
    public async Task Index_MapsAllProperties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = new DateTime(2025, 1, 15, 10, 30, 0);
        var apiResponse = new ApiResponse<List<FuelTypeResponseDto>>
        {
            Success = true,
            Data = new List<FuelTypeResponseDto>
            {
                new()
                {
                    Id = id,
                    Name = "Premium Diesel",
                    PricePerLiter = 1500,
                    IsActive = false,
                    EBMRegistered = true,
                    EBMProductId = "ebm-123",
                    EBMVariantId = "var-456",
                    EBMSupplyPrice = 1200,
                    CreatedAt = createdAt
                }
            }
        };
        _fuelTypeServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeAssignableTo<List<Escale.Web.Models.FuelType>>().Subject;
        var fuelType = model.Single();
        fuelType.Id.Should().Be(id);
        fuelType.Name.Should().Be("Premium Diesel");
        fuelType.PricePerLiter.Should().Be(1500);
        fuelType.IsActive.Should().BeFalse();
        fuelType.IsEBMRegistered.Should().BeTrue();
        fuelType.EBMProductId.Should().Be("ebm-123");
        fuelType.EBMVariantId.Should().Be("var-456");
        fuelType.EBMSupplyPrice.Should().Be(1200);
        fuelType.CreatedAt.Should().Be(createdAt);
    }

    #endregion

    #region Create

    [Fact]
    public async Task Create_RedirectsToIndex_WithSuccessMessage_WhenSuccessful()
    {
        // Arrange
        var apiResponse = new ApiResponse<FuelTypeResponseDto>
        {
            Success = true,
            Data = new FuelTypeResponseDto { Name = "Diesel", EBMRegistered = false }
        };
        _fuelTypeServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateFuelTypeRequestDto>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.Create("Diesel", 1200, null);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
        _controller.TempData["SuccessMessage"].Should().NotBeNull();
        _controller.TempData["SuccessMessage"]!.ToString().Should().Contain("Diesel");
    }

    [Fact]
    public async Task Create_SetsEBMSuccessMessage_WhenEBMRegistered()
    {
        // Arrange
        var apiResponse = new ApiResponse<FuelTypeResponseDto>
        {
            Success = true,
            Data = new FuelTypeResponseDto { Name = "Diesel", EBMRegistered = true }
        };
        _fuelTypeServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateFuelTypeRequestDto>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _controller.Create("Diesel", 1200, 1000);

        // Assert
        _controller.TempData["SuccessMessage"]!.ToString().Should().Contain("EBM successfully");
    }

    [Fact]
    public async Task Create_SetsNonEBMMessage_WhenEBMNotRegistered()
    {
        // Arrange
        var apiResponse = new ApiResponse<FuelTypeResponseDto>
        {
            Success = true,
            Data = new FuelTypeResponseDto { Name = "Diesel", EBMRegistered = false }
        };
        _fuelTypeServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateFuelTypeRequestDto>()))
            .ReturnsAsync(apiResponse);

        // Act
        await _controller.Create("Diesel", 1200, null);

        // Assert
        _controller.TempData["SuccessMessage"]!.ToString().Should().Contain("EBM not enabled");
    }

    [Fact]
    public async Task Create_SetsErrorMessage_WhenApiFails()
    {
        // Arrange
        var apiResponse = new ApiResponse<FuelTypeResponseDto>
        {
            Success = false,
            Message = "Fuel type name already exists"
        };
        _fuelTypeServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateFuelTypeRequestDto>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.Create("Diesel", 1200, null);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
        _controller.TempData["ErrorMessage"]!.ToString().Should().Be("Fuel type name already exists");
    }

    [Fact]
    public async Task Create_PassesCorrectDto_ToService()
    {
        // Arrange
        CreateFuelTypeRequestDto? capturedDto = null;
        _fuelTypeServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateFuelTypeRequestDto>()))
            .Callback<CreateFuelTypeRequestDto>(dto => capturedDto = dto)
            .ReturnsAsync(new ApiResponse<FuelTypeResponseDto>
            {
                Success = true,
                Data = new FuelTypeResponseDto { EBMRegistered = false }
            });

        // Act
        await _controller.Create("Premium", 1500, 1200);

        // Assert
        capturedDto.Should().NotBeNull();
        capturedDto!.Name.Should().Be("Premium");
        capturedDto.PricePerLiter.Should().Be(1500);
        capturedDto.EBMSupplyPrice.Should().Be(1200);
    }

    #endregion

    #region Edit

    [Fact]
    public async Task Edit_RedirectsToIndex_WithSuccessMessage_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        _fuelTypeServiceMock.Setup(s => s.UpdateAsync(id, It.IsAny<UpdateFuelTypeRequestDto>()))
            .ReturnsAsync(new ApiResponse<FuelTypeResponseDto>
            {
                Success = true,
                Data = new FuelTypeResponseDto { Name = "Updated" }
            });

        // Act
        var result = await _controller.Edit(id, "Updated", 1300, true, null);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
        _controller.TempData["SuccessMessage"]!.ToString().Should().Contain("updated successfully");
    }

    [Fact]
    public async Task Edit_SetsErrorMessage_WhenApiFails()
    {
        // Arrange
        var id = Guid.NewGuid();
        _fuelTypeServiceMock.Setup(s => s.UpdateAsync(id, It.IsAny<UpdateFuelTypeRequestDto>()))
            .ReturnsAsync(new ApiResponse<FuelTypeResponseDto>
            {
                Success = false,
                Message = "Failed to update price in EBM"
            });

        // Act
        await _controller.Edit(id, "Updated", 1300, true, 1100);

        // Assert
        _controller.TempData["ErrorMessage"]!.ToString().Should().Contain("EBM");
    }

    [Fact]
    public async Task Edit_PassesCorrectDto_ToService()
    {
        // Arrange
        var id = Guid.NewGuid();
        UpdateFuelTypeRequestDto? capturedDto = null;
        _fuelTypeServiceMock.Setup(s => s.UpdateAsync(id, It.IsAny<UpdateFuelTypeRequestDto>()))
            .Callback<Guid, UpdateFuelTypeRequestDto>((_, dto) => capturedDto = dto)
            .ReturnsAsync(new ApiResponse<FuelTypeResponseDto> { Success = true });

        // Act
        await _controller.Edit(id, "Premium", 1600, false, 1300);

        // Assert
        capturedDto.Should().NotBeNull();
        capturedDto!.Name.Should().Be("Premium");
        capturedDto.PricePerLiter.Should().Be(1600);
        capturedDto.IsActive.Should().BeFalse();
        capturedDto.EBMSupplyPrice.Should().Be(1300);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_RedirectsToIndex_WithSuccessMessage_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        _fuelTypeServiceMock.Setup(s => s.DeleteAsync(id))
            .ReturnsAsync(new ApiResponse { Success = true });

        // Act
        var result = await _controller.Delete(id);

        // Assert
        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
        _controller.TempData["SuccessMessage"]!.ToString().Should().Contain("deleted successfully");
    }

    [Fact]
    public async Task Delete_SetsErrorMessage_WhenApiFails()
    {
        // Arrange
        var id = Guid.NewGuid();
        _fuelTypeServiceMock.Setup(s => s.DeleteAsync(id))
            .ReturnsAsync(new ApiResponse { Success = false, Message = "Cannot delete" });

        // Act
        await _controller.Delete(id);

        // Assert
        _controller.TempData["ErrorMessage"]!.ToString().Should().Be("Cannot delete");
    }

    #endregion
}
