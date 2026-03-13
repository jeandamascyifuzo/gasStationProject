using System.Net;
using System.Text;
using System.Text.Json;
using Escale.Web.Models.Api;
using Escale.Web.Services;
using FluentAssertions;
using Xunit;

namespace Escale.Web.Tests.Services;

/// <summary>
/// Tests BaseApiService's protected methods via a testable subclass.
/// Covers all HTTP methods, error parsing paths, and edge cases.
/// </summary>
public class BaseApiServiceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = true
    };

    private static TestableApiService CreateService(HttpResponseMessage response)
    {
        var handler = new FakeHttpMessageHandler(response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7015") };
        return new TestableApiService(httpClient);
    }

    private static TestableApiService CreateService(Exception exception)
    {
        var handler = new FakeHttpMessageHandler(exception);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7015") };
        return new TestableApiService(httpClient);
    }

    #region PutAsync<T>

    [Fact]
    public async Task PutAsyncGeneric_ReturnsSuccess()
    {
        var apiResponse = new ApiResponse<string> { Success = true, Data = "updated" };
        var json = JsonSerializer.Serialize(apiResponse, JsonOptions);
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestPutAsync<string>("/api/test/1", new { Name = "test" });

        result.Success.Should().BeTrue();
        result.Data.Should().Be("updated");
    }

    [Fact]
    public async Task PutAsyncGeneric_ReturnsError_OnFailure()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("{\"Message\":\"Server error\"}", Encoding.UTF8, "application/json")
        });

        var result = await service.TestPutAsync<string>("/api/test/1", new { });

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task PutAsyncGeneric_HandlesException()
    {
        var service = CreateService(new HttpRequestException("Connection refused"));

        var result = await service.TestPutAsync<string>("/api/test/1", new { });

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Unable to connect");
    }

    #endregion

    #region PostAsync (non-generic)

    [Fact]
    public async Task PostAsyncNonGeneric_ReturnsSuccess()
    {
        var apiResponse = new ApiResponse { Success = true, Message = "Created" };
        var json = JsonSerializer.Serialize(apiResponse, JsonOptions);
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestPostAsync("/api/test", new { Name = "test" });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Created");
    }

    [Fact]
    public async Task PostAsyncNonGeneric_WithNullData_SendsEmptyBody()
    {
        var apiResponse = new ApiResponse { Success = true };
        var json = JsonSerializer.Serialize(apiResponse, JsonOptions);
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestPostAsync("/api/test", null);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task PostAsyncNonGeneric_ParsesErrorResponse()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"Message\":\"Validation failed\"}", Encoding.UTF8, "application/json")
        });

        var result = await service.TestPostAsync("/api/test", new { });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Validation failed");
    }

    [Fact]
    public async Task PostAsyncNonGeneric_ParsesProblemDetails_WithErrors()
    {
        var problemDetails = new
        {
            errors = new Dictionary<string, string[]>
            {
                { "Name", new[] { "Name is required" } }
            }
        };
        var json = JsonSerializer.Serialize(problemDetails, JsonOptions);
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestPostAsync("/api/test", new { });

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Name is required");
        result.Errors.Should().Contain("Name is required");
    }

    [Fact]
    public async Task PostAsyncNonGeneric_HandlesException()
    {
        var service = CreateService(new Exception("Something went wrong"));

        var result = await service.TestPostAsync("/api/test", new { });

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Something went wrong");
    }

    #endregion

    #region PutAsync (non-generic)

    [Fact]
    public async Task PutAsyncNonGeneric_ReturnsSuccess()
    {
        var apiResponse = new ApiResponse { Success = true, Message = "Updated" };
        var json = JsonSerializer.Serialize(apiResponse, JsonOptions);
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestPutAsync("/api/test/1", new { Name = "updated" });

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Updated");
    }

    [Fact]
    public async Task PutAsyncNonGeneric_ParsesErrorResponse()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"message\":\"Bad request\"}", Encoding.UTF8, "application/json")
        });

        var result = await service.TestPutAsync("/api/test/1", new { });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Bad request");
    }

    [Fact]
    public async Task PutAsyncNonGeneric_HandlesException()
    {
        var service = CreateService(new TaskCanceledException("Timeout"));

        var result = await service.TestPutAsync("/api/test/1", new { });

        result.Success.Should().BeFalse();
    }

    #endregion

    #region GetBytesAsync

    [Fact]
    public async Task GetBytesAsync_ReturnsBytes_OnSuccess()
    {
        var bytes = new byte[] { 1, 2, 3, 4, 5 };
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(bytes)
        });

        var result = await service.TestGetBytesAsync("/api/reports/download");

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(bytes);
    }

    [Fact]
    public async Task GetBytesAsync_ReturnsNull_OnFailure()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("Not found")
        });

        var result = await service.TestGetBytesAsync("/api/reports/download");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBytesAsync_ReturnsNull_OnException()
    {
        var service = CreateService(new HttpRequestException("Connection refused"));

        var result = await service.TestGetBytesAsync("/api/reports/download");

        result.Should().BeNull();
    }

    #endregion

    #region ParseErrorResponse edge cases

    [Fact]
    public async Task HandleResponse_ParsesErrorAsApiResponse_WhenPossible()
    {
        // API returns a proper ApiResponse<T> with Success=false on error
        var apiResponse = new ApiResponse<string>
        {
            Success = false,
            Message = "Not authorized",
            Errors = new List<string> { "Token expired" }
        };
        var json = JsonSerializer.Serialize(apiResponse, JsonOptions);
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestGetAsync<string>("/api/test");

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Not authorized");
    }

    [Fact]
    public async Task ParseErrorResponse_HandlesTitle_FromProblemDetails()
    {
        // Use non-generic PostAsync to go directly to ParseErrorResponse (not HandleResponse<T>
        // which would deserialize the JSON as ApiResponse<T> first and bypass ParseErrorResponse)
        var problemDetails = new { title = "Forbidden", status = 403 };
        var json = JsonSerializer.Serialize(problemDetails, JsonOptions);
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestPostAsync("/api/test", new { });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Forbidden");
    }

    [Fact]
    public async Task ParseErrorResponse_FallsBackToStatusCode_WhenUnparseable()
    {
        // Return plain text that can't be parsed as JSON
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("not json at all", Encoding.UTF8, "text/plain")
        });

        var result = await service.TestGetAsync<string>("/api/test");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("BadGateway");
    }

    [Fact]
    public async Task ErrorResponse_HandlesGenericException()
    {
        var service = CreateService(new InvalidOperationException("Unexpected error"));

        var result = await service.TestGetAsync<string>("/api/test");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Unexpected error");
    }

    [Fact]
    public async Task HandleResponse_ReturnsDefault_WhenNullDeserialization()
    {
        // Response is empty JSON
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        });

        var result = await service.TestGetAsync<string>("/api/test");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to deserialize");
    }

    [Fact]
    public async Task DeleteAsync_FallsBackToStatusCode_OnError()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("plain error text", Encoding.UTF8, "text/plain")
        });

        var result = await service.TestDeleteAsync("/api/test/1");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("InternalServerError");
    }

    [Fact]
    public async Task DeleteAsync_HandlesException()
    {
        var service = CreateService(new HttpRequestException("Network error"));

        var result = await service.TestDeleteAsync("/api/test/1");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Network error");
    }

    [Fact]
    public async Task DeleteAsync_ReturnsDefault_WhenNullDeserialization()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        });

        var result = await service.TestDeleteAsync("/api/test/1");

        result.Success.Should().BeTrue(); // fallback: new ApiResponse { Success = true }
    }

    [Fact]
    public async Task PostAsyncGeneric_WithNullData()
    {
        var apiResponse = new ApiResponse<string> { Success = true, Data = "ok" };
        var json = JsonSerializer.Serialize(apiResponse, JsonOptions);
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestPostAsync<string>("/api/test", null);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ParseErrorResponse_LowercaseMessage()
    {
        var json = "{\"message\":\"lowercase error\"}";
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestPostAsync("/api/test", new { });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("lowercase error");
    }

    [Fact]
    public async Task ParseErrorResponse_EmptyErrorsArray_FallsThrough()
    {
        // errors object with empty arrays → messages.Count == 0 → falls through to Message/title
        var json = "{\"errors\":{\"Field\":[]},\"Message\":\"Fallback message\"}";
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestPostAsync("/api/test", new { });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Fallback message");
    }

    [Fact]
    public async Task ParseErrorResponse_NullErrorMessage_FallsBackToFieldName()
    {
        // null element in errors array triggers msg.GetString() ?? field.Name
        var json = "{\"errors\":{\"FieldName\":[null]}}";
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestPostAsync("/api/test", new { });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("FieldName");
        result.Errors.Should().Contain("FieldName");
    }

    [Fact]
    public async Task ParseErrorResponse_NullMessageValue_FallsBackToStatusCode()
    {
        // {"Message": null} → msgProp.GetString() returns null → fallback to status code
        var json = "{\"Message\":null}";
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestPostAsync("/api/test", new { });

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("BadRequest");
    }

    [Fact]
    public async Task ParseErrorResponse_NullTitleValue_FallsBackToStatusCode()
    {
        // {"title": null} → titleProp.GetString() returns null → fallback to status code
        var json = "{\"title\":null}";
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestPostAsync("/api/test", new { });

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Forbidden");
    }

    [Fact]
    public async Task HandleResponse_ErrorFallsThrough_WhenDeserializationReturnsNull()
    {
        // Error response with "null" JSON → Deserialize<ApiResponse<T>> returns null → falls to ParseErrorResponse
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        });

        var result = await service.TestGetAsync<string>("/api/test");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("BadRequest");
    }

    [Fact]
    public async Task HandleResponse_ErrorFallsThrough_WhenDeserializationThrows()
    {
        // Error response with invalid JSON for ApiResponse<T> → catch block → ParseErrorResponse
        var json = "[1, 2, 3]"; // valid JSON but not deserializable as ApiResponse<string>
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestGetAsync<string>("/api/test");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("BadRequest");
    }

    [Fact]
    public async Task PostAsyncNonGeneric_NullDeserialization_ReturnsDefault()
    {
        // Success response with "null" → Deserialize<ApiResponse> returns null → fallback
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        });

        var result = await service.TestPostAsync("/api/test", new { });

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task PutAsyncNonGeneric_NullDeserialization_ReturnsDefault()
    {
        // Success response with "null" → Deserialize<ApiResponse> returns null → fallback
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        });

        var result = await service.TestPutAsync("/api/test/1", new { });

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task PutAsyncNonGeneric_ParsesProblemDetails_WithErrors()
    {
        // PutAsync non-generic also calls ParseErrorResponse on error
        var problemDetails = new
        {
            errors = new Dictionary<string, string[]>
            {
                { "Price", new[] { "Price must be positive" } }
            }
        };
        var json = JsonSerializer.Serialize(problemDetails, JsonOptions);
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestPutAsync("/api/test/1", new { });

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Price must be positive");
    }

    [Fact]
    public async Task PostAsyncGeneric_ReturnsSuccess_WithData()
    {
        var apiResponse = new ApiResponse<string> { Success = true, Data = "created" };
        var json = JsonSerializer.Serialize(apiResponse, JsonOptions);
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestPostAsync<string>("/api/test", new { Name = "test" });

        result.Success.Should().BeTrue();
        result.Data.Should().Be("created");
    }

    [Fact]
    public async Task PostAsyncGeneric_ReturnsError_OnFailure()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("{\"Message\":\"Server error\"}", Encoding.UTF8, "application/json")
        });

        var result = await service.TestPostAsync<string>("/api/test", new { });

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task PostAsyncGeneric_HandlesException()
    {
        var service = CreateService(new HttpRequestException("Connection refused"));

        var result = await service.TestPostAsync<string>("/api/test", new { });

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Unable to connect");
    }

    [Fact]
    public async Task ParseErrorResponse_EmptyErrorsOnly_FallsBackToStatusCode()
    {
        // errors object with empty arrays, no Message or title → falls all the way to status code
        var json = "{\"errors\":{\"Field\":[]}}";
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestPostAsync("/api/test", new { });

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("UnprocessableEntity");
    }

    [Fact]
    public async Task ParseErrorResponse_ErrorsNotObject_FallsToMessage()
    {
        // "errors" exists but is not an Object (it's a string) → skip errors block → use Message
        var json = "{\"errors\":\"not an object\",\"Message\":\"Use this\"}";
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var result = await service.TestPutAsync("/api/test/1", new { });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Use this");
    }

    #endregion
}

/// <summary>
/// Exposes BaseApiService's protected methods for testing.
/// </summary>
internal class TestableApiService : BaseApiService
{
    public TestableApiService(HttpClient httpClient) : base(httpClient) { }

    public Task<ApiResponse<T>> TestGetAsync<T>(string endpoint) => GetAsync<T>(endpoint);
    public Task<ApiResponse<T>> TestPostAsync<T>(string endpoint, object? data) => PostAsync<T>(endpoint, data);
    public Task<ApiResponse<T>> TestPutAsync<T>(string endpoint, object data) => PutAsync<T>(endpoint, data);
    public Task<ApiResponse> TestDeleteAsync(string endpoint) => DeleteAsync(endpoint);
    public Task<ApiResponse> TestPostAsync(string endpoint, object? data) => PostAsync(endpoint, data);
    public Task<ApiResponse> TestPutAsync(string endpoint, object data) => PutAsync(endpoint, data);
    public Task<byte[]?> TestGetBytesAsync(string endpoint) => GetBytesAsync(endpoint);
}
