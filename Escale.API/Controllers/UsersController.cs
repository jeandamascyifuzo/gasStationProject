using Escale.API.DTOs.Common;
using Escale.API.DTOs.Users;
using Escale.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Escale.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserResponseDto>>>> GetUsers([FromQuery] PagedRequest request)
    {
        var result = await _userService.GetUsersAsync(request);
        return Ok(ApiResponse<PagedResult<UserResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetUser(Guid id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return Ok(ApiResponse<UserResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> CreateUser([FromBody] CreateUserRequestDto request)
    {
        var result = await _userService.CreateUserAsync(request);
        return Ok(ApiResponse<UserResponseDto>.SuccessResponse(result, "User created"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> UpdateUser(Guid id, [FromBody] UpdateUserRequestDto request)
    {
        var result = await _userService.UpdateUserAsync(id, request);
        return Ok(ApiResponse<UserResponseDto>.SuccessResponse(result, "User updated"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteUser(Guid id)
    {
        await _userService.DeleteUserAsync(id);
        return Ok(ApiResponse.SuccessResponse("User deleted"));
    }

    [HttpPost("{id}/change-password")]
    public async Task<ActionResult<ApiResponse>> ChangePassword(Guid id, [FromBody] ChangePasswordRequestDto request)
    {
        await _userService.ChangePasswordAsync(id, request);
        return Ok(ApiResponse.SuccessResponse("Password changed"));
    }

    [HttpPost("{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse>> ToggleStatus(Guid id)
    {
        await _userService.ToggleStatusAsync(id);
        return Ok(ApiResponse.SuccessResponse("Status toggled"));
    }
}
