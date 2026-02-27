using Escale.Web.Helpers;

namespace Escale.Web.Handlers;

public class AuthenticatedHttpClientHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticatedHttpClientHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var token = httpContext.Request.Cookies[TokenHelper.AccessTokenCookie];
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && httpContext != null)
        {
            var refreshToken = httpContext.Request.Cookies[TokenHelper.RefreshTokenCookie];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var newToken = await TryRefreshTokenAsync(httpContext, refreshToken, cancellationToken);
                if (!string.IsNullOrEmpty(newToken))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newToken);
                    response = await base.SendAsync(request, cancellationToken);
                }
            }
        }

        return response;
    }

    private async Task<string?> TryRefreshTokenAsync(HttpContext httpContext, string refreshToken, CancellationToken cancellationToken)
    {
        try
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            using var client = new HttpClient(handler);
            var baseUrl = httpContext.RequestServices
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<Configuration.ApiSettings>>()
                .Value.BaseUrl;

            var requestBody = new { RefreshToken = refreshToken };
            var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{baseUrl}/auth/refresh-token", content, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = System.Text.Json.JsonSerializer.Deserialize<Models.Api.AuthDtos.LoginResponseDto>(responseJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result?.Success == true)
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                };
                httpContext.Response.Cookies.Append(TokenHelper.AccessTokenCookie, result.Token, cookieOptions);
                httpContext.Response.Cookies.Append(TokenHelper.RefreshTokenCookie, result.RefreshToken, cookieOptions);
                return result.Token;
            }
        }
        catch
        {
            // Refresh failed, user will need to re-login
        }
        return null;
    }
}
