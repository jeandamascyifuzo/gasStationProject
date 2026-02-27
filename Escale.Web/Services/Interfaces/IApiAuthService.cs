using Escale.Web.Models.Api.AuthDtos;

namespace Escale.Web.Services.Interfaces;

public interface IApiAuthService
{
    Task<LoginResponseDto> LoginAsync(string username, string password, bool rememberMe);
    Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken, string accessToken);
}
