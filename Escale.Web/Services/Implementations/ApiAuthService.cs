using System.Text;
using System.Text.Json;
using Escale.Web.Models.Api.AuthDtos;
using Escale.Web.Services.Interfaces;

namespace Escale.Web.Services.Implementations;

public class ApiAuthService : IApiAuthService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = true
    };

    public ApiAuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResponseDto> LoginAsync(string username, string password, bool rememberMe)
    {
        try
        {
            var request = new LoginRequestDto
            {
                Username = username,
                Password = password,
                RememberMe = rememberMe
            };

            var json = JsonSerializer.Serialize(request, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/auth/login", content);
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<LoginResponseDto>(responseJson, JsonOptions);
            return result ?? new LoginResponseDto { Success = false, Message = "Failed to deserialize response" };
        }
        catch (HttpRequestException)
        {
            return new LoginResponseDto { Success = false, Message = "Unable to connect to the API server." };
        }
        catch (Exception ex)
        {
            return new LoginResponseDto { Success = false, Message = $"An error occurred: {ex.Message}" };
        }
    }

    public async Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var request = new RefreshTokenRequestDto { RefreshToken = refreshToken };
            var json = JsonSerializer.Serialize(request, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/auth/refresh-token", content);

            if (!response.IsSuccessStatusCode) return null;

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LoginResponseDto>(responseJson, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken, string accessToken)
    {
        try
        {
            var request = new RefreshTokenRequestDto { RefreshToken = refreshToken };
            var json = JsonSerializer.Serialize(request, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/revoke-token")
            {
                Content = content
            };
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(httpRequest);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
