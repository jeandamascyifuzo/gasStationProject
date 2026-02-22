using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Entities;
using Escale.API.Domain.Enums;
using Escale.API.DTOs.Common;
using Escale.API.DTOs.Users;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<PagedResult<UserResponseDto>> GetUsersAsync(PagedRequest request)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var query = _unitOfWork.Users.Query()
            .Include(u => u.UserStations).ThenInclude(us => us.Station)
            .Where(u => u.OrganizationId == orgId);

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(u => u.Username.ToLower().Contains(term) ||
                                     u.FullName.ToLower().Contains(term) ||
                                     (u.Email != null && u.Email.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.FullName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResult<UserResponseDto>
        {
            Items = _mapper.Map<List<UserResponseDto>>(users),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<UserResponseDto> GetUserByIdAsync(Guid id)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var user = await _unitOfWork.Users.Query()
            .Include(u => u.UserStations).ThenInclude(us => us.Station)
            .FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("User not found");
        return _mapper.Map<UserResponseDto>(user);
    }

    public async Task<UserResponseDto> CreateUserAsync(CreateUserRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;

        if (await _unitOfWork.Users.ExistsAsync(u => u.OrganizationId == orgId && u.Username == request.Username))
            throw new InvalidOperationException("Username already exists");

        var user = new User
        {
            OrganizationId = orgId,
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            Role = Enum.Parse<UserRole>(request.Role),
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Assign stations
        if (request.StationIds.Any())
        {
            foreach (var stationId in request.StationIds)
            {
                _unitOfWork.Context.UserStations.Add(new UserStation
                {
                    UserId = user.Id,
                    StationId = stationId,
                    AssignedAt = DateTime.UtcNow
                });
            }
            await _unitOfWork.SaveChangesAsync();
        }

        return await GetUserByIdAsync(user.Id);
    }

    public async Task<UserResponseDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var user = await _unitOfWork.Users.Query()
            .Include(u => u.UserStations)
            .FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("User not found");

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Phone = request.Phone;
        user.Role = Enum.Parse<UserRole>(request.Role);

        // Update station assignments
        _unitOfWork.Context.UserStations.RemoveRange(user.UserStations);
        foreach (var stationId in request.StationIds)
        {
            _unitOfWork.Context.UserStations.Add(new UserStation
            {
                UserId = user.Id,
                StationId = stationId,
                AssignedAt = DateTime.UtcNow
            });
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        return await GetUserByIdAsync(user.Id);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("User not found");
        _unitOfWork.Users.Remove(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("User not found");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("Current password is incorrect");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ToggleStatusAsync(Guid id)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("User not found");
        user.IsActive = !user.IsActive;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }
}
