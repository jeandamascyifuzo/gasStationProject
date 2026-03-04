using Escale.API.DTOs.Auth;
using Escale.API.DTOs.Common;
using Escale.API.DTOs.Users;
using Escale.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirst("UserId")!.Value);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.Success) return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        if (!result.Success) return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> RevokeToken([FromBody] RefreshTokenRequestDto request)
    {
        await _authService.RevokeTokenAsync(request.RefreshToken);
        return Ok(ApiResponse.SuccessResponse("Token revoked"));
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ProfileResponseDto>>> GetProfile()
    {
        var result = await _authService.GetProfileAsync(GetUserId());
        return Ok(ApiResponse<ProfileResponseDto>.SuccessResponse(result));
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ProfileResponseDto>>> UpdateProfile([FromBody] UpdateProfileRequestDto request)
    {
        var result = await _authService.UpdateProfileAsync(GetUserId(), request);
        return Ok(ApiResponse<ProfileResponseDto>.SuccessResponse(result, "Profile updated"));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        await _authService.ChangeOwnPasswordAsync(GetUserId(), request);
        return Ok(ApiResponse.SuccessResponse("Password changed successfully"));
    }
}
