using System.Net;
using System.Text;
using System.Text.Json;
using Escale.Web.Models.Api;
using Escale.Web.Services.Implementations;
using FluentAssertions;
using Xunit;

namespace Escale.Web.Tests.Services;

/// <summary>
/// Tests the ApiFuelTypeService by providing a fake HttpMessageHandler.
/// This validates the BaseApiService HTTP handling without a real API.
/// </summary>
public class ApiFuelTypeServiceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = true
    };

    private static ApiFuelTypeService CreateService(HttpResponseMessage response)
    {
        var handler = new FakeHttpMessageHandler(response);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:7015")
        };
        return new ApiFuelTypeService(httpClient);
    }

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ReturnsSuccess_WithFuelTypes()
    {
        // Arrange
        var fuelTypes = new List<FuelTypeResponseDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Diesel", PricePerLiter = 1200, IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Petrol", PricePerLiter = 1350, IsActive = true }
        };
        var apiResponse = new ApiResponse<List<FuelTypeResponseDto>>
        {
            Success = true,
            Message = "Success",
            Data = fuelTypes
        };
        var json = JsonSerializer.Serialize(apiResponse, JsonOptions);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var service = CreateService(httpResponse);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data![0].Name.Should().Be("Diesel");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsError_WhenApiFails()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { Message = "Server error" }, JsonOptions),
                Encoding.UTF8, "application/json")
        };
        var service = CreateService(httpResponse);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ReturnsSuccess_WithFuelType()
    {
        // Arrange
        var id = Guid.NewGuid();
        var apiResponse = new ApiResponse<FuelTypeResponseDto>
        {
            Success = true,
            Data = new FuelTypeResponseDto { Id = id, Name = "Diesel", PricePerLiter = 1200 }
        };
        var json = JsonSerializer.Serialize(apiResponse, JsonOptions);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var service = CreateService(httpResponse);

        // Act
        var result = await service.GetByIdAsync(id);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Id.Should().Be(id);
        result.Data.Name.Should().Be("Diesel");
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ReturnsSuccess_WhenCreated()
    {
        // Arrange
        var apiResponse = new ApiResponse<FuelTypeResponseDto>
        {
            Success = true,
            Data = new FuelTypeResponseDto
            {
                Id = Guid.NewGuid(),
                Name = "Premium",
                PricePerLiter = 1500,
                EBMRegistered = true
            }
        };
        var json = JsonSerializer.Serialize(apiResponse, JsonOptions);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var service = CreateService(httpResponse);

        var request = new CreateFuelTypeRequestDto
        {
            Name = "Premium",
            PricePerLiter = 1500,
            EBMSupplyPrice = 1200
        };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Premium");
        result.Data.EBMRegistered.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ReturnsError_WhenValidationFails()
    {
        // Arrange - ProblemDetails format from ASP.NET validation
        var problemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title = "One or more validation errors occurred.",
            status = 400,
            errors = new Dictionary<string, string[]>
            {
                { "Name", new[] { "Name is required" } },
                { "PricePerLiter", new[] { "Price must be greater than 0" } }
            }
        };
        var json = JsonSerializer.Serialize(problemDetails, JsonOptions);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var service = CreateService(httpResponse);

        // Act
        var result = await service.CreateAsync(new CreateFuelTypeRequestDto());

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ReturnsSuccess_WhenDeleted()
    {
        // Arrange
        var apiResponse = new ApiResponse { Success = true, Message = "Fuel type deleted" };
        var json = JsonSerializer.Serialize(apiResponse, JsonOptions);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var service = CreateService(httpResponse);

        // Act
        var result = await service.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsError_WhenNotFound()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { Message = "Not found" }, JsonOptions),
                Encoding.UTF8, "application/json")
        };
        var service = CreateService(httpResponse);

        // Act
        var result = await service.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Success.Should().BeFalse();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ReturnsSuccess_WhenUpdated()
    {
        // Arrange
        var id = Guid.NewGuid();
        var apiResponse = new ApiResponse<FuelTypeResponseDto>
        {
            Success = true,
            Data = new FuelTypeResponseDto { Id = id, Name = "Updated", PricePerLiter = 1500 }
        };
        var json = JsonSerializer.Serialize(apiResponse, JsonOptions);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var service = CreateService(httpResponse);

        var request = new UpdateFuelTypeRequestDto
        {
            Name = "Updated",
            PricePerLiter = 1500,
            IsActive = true,
            EBMSupplyPrice = 1200
        };

        // Act
        var result = await service.UpdateAsync(id, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Updated");
        result.Data.PricePerLiter.Should().Be(1500);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsError_WhenApiFails()
    {
        // Arrange
        var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new { Message = "EBM update failed" }, JsonOptions),
                Encoding.UTF8, "application/json")
        };
        var service = CreateService(httpResponse);

        // Act
        var result = await service.UpdateAsync(Guid.NewGuid(), new UpdateFuelTypeRequestDto());

        // Assert
        result.Success.Should().BeFalse();
    }

    #endregion

    #region Error handling

    [Fact]
    public async Task GetAllAsync_HandlesConnectionError_Gracefully()
    {
        // Arrange - simulate a network failure
        var handler = new FakeHttpMessageHandler(new HttpRequestException("Connection refused"));
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:7015")
        };
        var service = new ApiFuelTypeService(httpClient);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Unable to connect");
    }

    [Fact]
    public async Task GetAllAsync_HandlesTimeout_Gracefully()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(new TaskCanceledException("Timeout"));
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://localhost:7015")
        };
        var service = new ApiFuelTypeService(httpClient);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("timed out");
    }

    #endregion
}

/// <summary>
/// Fake HttpMessageHandler for testing BaseApiService-derived services.
/// Can be configured to return a specific response or throw an exception.
/// </summary>
internal class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage? _response;
    private readonly Exception? _exception;

    public FakeHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    public FakeHttpMessageHandler(Exception exception)
    {
        _exception = exception;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_exception != null)
            throw _exception;
        return Task.FromResult(_response!);
    }
}
