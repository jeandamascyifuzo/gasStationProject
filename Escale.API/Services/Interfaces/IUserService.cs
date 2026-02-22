using Escale.API.DTOs.Common;
using Escale.API.DTOs.Users;

namespace Escale.API.Services.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserResponseDto>> GetUsersAsync(PagedRequest request);
    Task<UserResponseDto> GetUserByIdAsync(Guid id);
    Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto request);
    Task<UserResponseDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request);
    Task DeleteUserAsync(Guid id);
    Task ChangePasswordAsync(Guid id, ChangePasswordRequestDto request);
    Task ToggleStatusAsync(Guid id);
}
