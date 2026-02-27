using System.Text;
using System.Text.Json;
using Escale.Web.Models.Api;

namespace Escale.Web.Services;

public abstract class BaseApiService
{
    protected readonly HttpClient HttpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null, // PascalCase to match API
        PropertyNameCaseInsensitive = true
    };

    protected BaseApiService(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    protected async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await HttpClient.GetAsync(endpoint);
            return await HandleResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ErrorResponse<T>(ex);
        }
    }

    protected async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null)
    {
        try
        {
            var content = data != null
                ? new StringContent(JsonSerializer.Serialize(data, JsonOptions), Encoding.UTF8, "application/json")
                : null;
            var response = await HttpClient.PostAsync(endpoint, content);
            return await HandleResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ErrorResponse<T>(ex);
        }
    }

    protected async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(data, JsonOptions), Encoding.UTF8, "application/json");
            var response = await HttpClient.PutAsync(endpoint, content);
            return await HandleResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ErrorResponse<T>(ex);
        }
    }

    protected async Task<ApiResponse> DeleteAsync(string endpoint)
    {
        try
        {
            var response = await HttpClient.DeleteAsync(endpoint);
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse>(json, JsonOptions);
                return result ?? new ApiResponse { Success = true };
            }
            return new ApiResponse { Success = false, Message = $"API error: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            return new ApiResponse { Success = false, Message = ex.Message };
        }
    }

    protected async Task<ApiResponse> PostAsync(string endpoint, object? data = null)
    {
        try
        {
            var content = data != null
                ? new StringContent(JsonSerializer.Serialize(data, JsonOptions), Encoding.UTF8, "application/json")
                : null;
            var response = await HttpClient.PostAsync(endpoint, content);
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse>(json, JsonOptions);
                return result ?? new ApiResponse { Success = true };
            }
            return new ApiResponse { Success = false, Message = $"API error: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            return new ApiResponse { Success = false, Message = ex.Message };
        }
    }

    protected async Task<ApiResponse> PutAsync(string endpoint, object data)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(data, JsonOptions), Encoding.UTF8, "application/json");
            var response = await HttpClient.PutAsync(endpoint, content);
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResponse>(json, JsonOptions);
                return result ?? new ApiResponse { Success = true };
            }
            return new ApiResponse { Success = false, Message = $"API error: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            return new ApiResponse { Success = false, Message = ex.Message };
        }
    }

    protected async Task<byte[]?> GetBytesAsync(string endpoint)
    {
        try
        {
            var response = await HttpClient.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsByteArrayAsync();
            return null;
        }
        catch
        {
            return null;
        }
    }

    private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            var result = JsonSerializer.Deserialize<ApiResponse<T>>(json, JsonOptions);
            return result ?? new ApiResponse<T> { Success = false, Message = "Failed to deserialize response" };
        }

        // Try to parse error response
        try
        {
            var errorResult = JsonSerializer.Deserialize<ApiResponse<T>>(json, JsonOptions);
            if (errorResult != null) return errorResult;
        }
        catch { }

        return new ApiResponse<T>
        {
            Success = false,
            Message = $"API error: {response.StatusCode}",
            Errors = new List<string> { json }
        };
    }

    private static ApiResponse<T> ErrorResponse<T>(Exception ex)
    {
        var message = ex switch
        {
            HttpRequestException => "Unable to connect to the API server. Please try again later.",
            TaskCanceledException => "The request timed out. Please try again.",
            _ => $"An error occurred: {ex.Message}"
        };
        return new ApiResponse<T> { Success = false, Message = message };
    }
}
