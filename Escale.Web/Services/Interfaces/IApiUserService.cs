using Escale.Web.Models.Api;

namespace Escale.Web.Services.Interfaces;

public interface IApiUserService
{
    Task<ApiResponse<PagedResult<UserResponseDto>>> GetAllAsync(int page = 1, int pageSize = 20, string? searchTerm = null);
    Task<ApiResponse<UserResponseDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<UserResponseDto>> CreateAsync(CreateUserRequestDto request);
    Task<ApiResponse<UserResponseDto>> UpdateAsync(Guid id, UpdateUserRequestDto request);
    Task<ApiResponse> DeleteAsync(Guid id);
    Task<ApiResponse> ChangePasswordAsync(Guid id, ChangePasswordRequestDto request);
    Task<ApiResponse> ToggleStatusAsync(Guid id);
}
