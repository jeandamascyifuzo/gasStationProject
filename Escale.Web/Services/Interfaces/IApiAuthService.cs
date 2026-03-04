using Escale.Web.Models.Api;
using Escale.Web.Models.Api.AuthDtos;

namespace Escale.Web.Services.Interfaces;

public interface IApiAuthService
{
    Task<LoginResponseDto> LoginAsync(string username, string password, bool rememberMe);
    Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken, string accessToken);
    Task<ApiResponse<ProfileResponseDto>> GetProfileAsync(string accessToken);
    Task<ApiResponse<ProfileResponseDto>> UpdateProfileAsync(UpdateProfileRequestDto request, string accessToken);
    Task<ApiResponse> ChangePasswordAsync(ChangePasswordRequestDto request, string accessToken);
}
