using Escale.API.DTOs.Common;
using FluentAssertions;
using Xunit;

namespace Escale.API.Tests.DTOs;

public class ApiResponseTests
{
    [Fact]
    public void GenericSuccessResponse_SetsAllProperties()
    {
        var response = ApiResponse<string>.SuccessResponse("data", "Custom message");

        response.Success.Should().BeTrue();
        response.Message.Should().Be("Custom message");
        response.Data.Should().Be("data");
        response.Errors.Should().BeNull();
    }

    [Fact]
    public void GenericSuccessResponse_DefaultMessage()
    {
        var response = ApiResponse<int>.SuccessResponse(42);

        response.Success.Should().BeTrue();
        response.Message.Should().Be("Success");
        response.Data.Should().Be(42);
    }

    [Fact]
    public void GenericErrorResponse_SetsAllProperties()
    {
        var errors = new List<string> { "Error 1", "Error 2" };
        var response = ApiResponse<string>.ErrorResponse("Something failed", errors);

        response.Success.Should().BeFalse();
        response.Message.Should().Be("Something failed");
        response.Errors.Should().HaveCount(2);
        response.Data.Should().BeNull();
    }

    [Fact]
    public void GenericErrorResponse_WithoutErrors()
    {
        var response = ApiResponse<string>.ErrorResponse("Failed");

        response.Success.Should().BeFalse();
        response.Message.Should().Be("Failed");
        response.Errors.Should().BeNull();
    }

    [Fact]
    public void NonGenericSuccessResponse_SetsProperties()
    {
        var response = ApiResponse.SuccessResponse("Done");

        response.Success.Should().BeTrue();
        response.Message.Should().Be("Done");
        response.Errors.Should().BeNull();
    }

    [Fact]
    public void NonGenericSuccessResponse_DefaultMessage()
    {
        var response = ApiResponse.SuccessResponse();

        response.Success.Should().BeTrue();
        response.Message.Should().Be("Success");
    }

    [Fact]
    public void NonGenericErrorResponse_SetsAllProperties()
    {
        var errors = new List<string> { "Validation error" };
        var response = ApiResponse.ErrorResponse("Bad request", errors);

        response.Success.Should().BeFalse();
        response.Message.Should().Be("Bad request");
        response.Errors.Should().HaveCount(1);
    }

    [Fact]
    public void NonGenericErrorResponse_WithoutErrors()
    {
        var response = ApiResponse.ErrorResponse("Internal error");

        response.Success.Should().BeFalse();
        response.Message.Should().Be("Internal error");
        response.Errors.Should().BeNull();
    }
}
