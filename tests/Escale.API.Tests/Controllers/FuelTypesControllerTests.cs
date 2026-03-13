using Escale.API.Controllers;
using Escale.API.DTOs.Common;
using Escale.API.DTOs.FuelTypes;
using Escale.API.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Escale.API.Tests.Controllers;

public class FuelTypesControllerTests
{
    private readonly Mock<IFuelTypeService> _fuelTypeServiceMock;
    private readonly FuelTypesController _controller;

    public FuelTypesControllerTests()
    {
        _fuelTypeServiceMock = new Mock<IFuelTypeService>();
        _controller = new FuelTypesController(_fuelTypeServiceMock.Object);
    }

    #region GetFuelTypes

    [Fact]
    public async Task GetFuelTypes_ReturnsOk_WithListOfFuelTypes()
    {
        // Arrange
        var fuelTypes = new List<FuelTypeResponseDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Diesel", PricePerLiter = 1200, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Petrol", PricePerLiter = 1350, IsActive = true }
        };
        _fuelTypeServiceMock.Setup(s => s.GetFuelTypesAsync()).ReturnsAsync(fuelTypes);

        // Act
        var result = await _controller.GetFuelTypes();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<List<FuelTypeResponseDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(2);
        response.Data![0].Name.Should().Be("Diesel");
        response.Data[1].Name.Should().Be("Petrol");
    }

    [Fact]
    public async Task GetFuelTypes_ReturnsOk_WithEmptyList_WhenNoFuelTypes()
    {
        // Arrange
        _fuelTypeServiceMock.Setup(s => s.GetFuelTypesAsync()).ReturnsAsync(new List<FuelTypeResponseDto>());

        // Act
        var result = await _controller.GetFuelTypes();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<List<FuelTypeResponseDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEmpty();
    }

    #endregion

    #region GetFuelType (by ID)

    [Fact]
    public async Task GetFuelType_ReturnsOk_WhenFuelTypeExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fuelType = new FuelTypeResponseDto
        {
            Id = id,
            Name = "Diesel",
            PricePerLiter = 1200,
            IsActive = true,
            EBMRegistered = true,
            EBMProductId = "prod-123"
        };
        _fuelTypeServiceMock.Setup(s => s.GetFuelTypeByIdAsync(id)).ReturnsAsync(fuelType);

        // Act
        var result = await _controller.GetFuelType(id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<FuelTypeResponseDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Id.Should().Be(id);
        response.Data.Name.Should().Be("Diesel");
        response.Data.EBMRegistered.Should().BeTrue();
    }

    [Fact]
    public async Task GetFuelType_ThrowsKeyNotFoundException_WhenNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _fuelTypeServiceMock.Setup(s => s.GetFuelTypeByIdAsync(id))
            .ThrowsAsync(new KeyNotFoundException("Fuel type not found"));

        // Act
        Func<Task> act = async () => await _controller.GetFuelType(id);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Fuel type not found");
    }

    #endregion

    #region CreateFuelType

    [Fact]
    public async Task CreateFuelType_ReturnsOk_WithEBMMessage_WhenEBMRegistered()
    {
        // Arrange
        var request = new CreateFuelTypeRequestDto
        {
            Name = "Premium",
            PricePerLiter = 1500,
            EBMSupplyPrice = 1300
        };
        var response = new FuelTypeResponseDto
        {
            Id = Guid.NewGuid(),
            Name = "Premium",
            PricePerLiter = 1500,
            IsActive = true,
            EBMRegistered = true,
            EBMProductId = "ebm-prod-1"
        };
        _fuelTypeServiceMock.Setup(s => s.CreateFuelTypeAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _controller.CreateFuelType(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<FuelTypeResponseDto>>().Subject;
        apiResponse.Success.Should().BeTrue();
        apiResponse.Message.Should().Contain("EBM");
        apiResponse.Data!.Name.Should().Be("Premium");
    }

    [Fact]
    public async Task CreateFuelType_ReturnsOk_WithoutEBMMessage_WhenEBMNotRegistered()
    {
        // Arrange
        var request = new CreateFuelTypeRequestDto
        {
            Name = "Regular",
            PricePerLiter = 1100
        };
        var response = new FuelTypeResponseDto
        {
            Id = Guid.NewGuid(),
            Name = "Regular",
            PricePerLiter = 1100,
            IsActive = true,
            EBMRegistered = false
        };
        _fuelTypeServiceMock.Setup(s => s.CreateFuelTypeAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _controller.CreateFuelType(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<FuelTypeResponseDto>>().Subject;
        apiResponse.Success.Should().BeTrue();
        apiResponse.Message.Should().Be("Fuel type created");
    }

    [Fact]
    public async Task CreateFuelType_ThrowsInvalidOperation_WhenDuplicateName()
    {
        // Arrange
        var request = new CreateFuelTypeRequestDto { Name = "Diesel", PricePerLiter = 1200 };
        _fuelTypeServiceMock.Setup(s => s.CreateFuelTypeAsync(request))
            .ThrowsAsync(new InvalidOperationException("Fuel type name already exists"));

        // Act
        Func<Task> act = async () => await _controller.CreateFuelType(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Fuel type name already exists");
    }

    #endregion

    #region UpdateFuelType

    [Fact]
    public async Task UpdateFuelType_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateFuelTypeRequestDto
        {
            Name = "Diesel Updated",
            PricePerLiter = 1250,
            IsActive = true
        };
        var response = new FuelTypeResponseDto
        {
            Id = id,
            Name = "Diesel Updated",
            PricePerLiter = 1250,
            IsActive = true
        };
        _fuelTypeServiceMock.Setup(s => s.UpdateFuelTypeAsync(id, request)).ReturnsAsync(response);

        // Act
        var result = await _controller.UpdateFuelType(id, request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<FuelTypeResponseDto>>().Subject;
        apiResponse.Success.Should().BeTrue();
        apiResponse.Message.Should().Be("Fuel type updated");
        apiResponse.Data!.Name.Should().Be("Diesel Updated");
        apiResponse.Data.PricePerLiter.Should().Be(1250);
    }

    [Fact]
    public async Task UpdateFuelType_ThrowsKeyNotFound_WhenNotExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateFuelTypeRequestDto { Name = "X", PricePerLiter = 100, IsActive = true };
        _fuelTypeServiceMock.Setup(s => s.UpdateFuelTypeAsync(id, request))
            .ThrowsAsync(new KeyNotFoundException("Fuel type not found"));

        // Act
        Func<Task> act = async () => await _controller.UpdateFuelType(id, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region DeleteFuelType

    [Fact]
    public async Task DeleteFuelType_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        _fuelTypeServiceMock.Setup(s => s.DeleteFuelTypeAsync(id)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteFuelType(id);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse>().Subject;
        apiResponse.Success.Should().BeTrue();
        apiResponse.Message.Should().Be("Fuel type deleted");
    }

    [Fact]
    public async Task DeleteFuelType_ThrowsKeyNotFound_WhenNotExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        _fuelTypeServiceMock.Setup(s => s.DeleteFuelTypeAsync(id))
            .ThrowsAsync(new KeyNotFoundException("Fuel type not found"));

        // Act
        Func<Task> act = async () => await _controller.DeleteFuelType(id);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Service interaction verification

    [Fact]
    public async Task GetFuelTypes_CallsServiceExactlyOnce()
    {
        // Arrange
        _fuelTypeServiceMock.Setup(s => s.GetFuelTypesAsync()).ReturnsAsync(new List<FuelTypeResponseDto>());

        // Act
        await _controller.GetFuelTypes();

        // Assert
        _fuelTypeServiceMock.Verify(s => s.GetFuelTypesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteFuelType_CallsServiceWithCorrectId()
    {
        // Arrange
        var id = Guid.NewGuid();
        _fuelTypeServiceMock.Setup(s => s.DeleteFuelTypeAsync(id)).Returns(Task.CompletedTask);

        // Act
        await _controller.DeleteFuelType(id);

        // Assert
        _fuelTypeServiceMock.Verify(s => s.DeleteFuelTypeAsync(id), Times.Once);
    }

    #endregion
}
