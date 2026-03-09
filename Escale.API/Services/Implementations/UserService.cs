using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Entities;
using Escale.API.Domain.Enums;
using Escale.API.DTOs.Common;
using Escale.API.DTOs.Users;
using Escale.API.Hubs;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogger _audit;

    public UserService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IMapper mapper,
        INotificationService notificationService, IAuditLogger audit)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _notificationService = notificationService;
        _audit = audit;
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

        // Assign stations (only for Cashier and Supervisor)
        if (request.StationIds.Any() && (user.Role == UserRole.Cashier || user.Role == UserRole.Supervisor))
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

        await _audit.LogAsync("UserCreate", "User", user.Id.ToString(), new
        {
            Username = user.Username, FullName = user.FullName, Role = user.Role.ToString(),
            Email = user.Email, StationCount = request.StationIds.Count
        });

        return await GetUserByIdAsync(user.Id);
    }

    public async Task<UserResponseDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var user = await _unitOfWork.Users.Query()
            .Include(u => u.UserStations)
            .FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("User not found");

        // Manager cannot update Admin profiles
        if (_currentUser.Role == "Manager" && user.Role == UserRole.Admin)
            throw new InvalidOperationException("Managers cannot modify Admin profiles.");

        var oldRole = user.Role.ToString();
        var oldName = user.FullName;

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Phone = request.Phone;
        user.Role = Enum.Parse<UserRole>(request.Role);

        // Update station assignments (only for Cashier and Supervisor)
        _unitOfWork.Context.UserStations.RemoveRange(user.UserStations);
        if (user.Role == UserRole.Cashier || user.Role == UserRole.Supervisor)
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
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        await _audit.LogAsync("UserUpdate", "User", user.Id.ToString(), new
        {
            Username = user.Username,
            FullName = new { Before = oldName, After = user.FullName },
            Role = new { Before = oldRole, After = user.Role.ToString() },
            StationCount = request.StationIds.Count
        });

        _ = _notificationService.NotifyDataChangedAsync(orgId, NotificationConstants.UserChanged);
        return await GetUserByIdAsync(user.Id);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var orgId = _currentUser.OrganizationId!.Value;
        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId)
            ?? throw new KeyNotFoundException("User not found");

        // Manager cannot delete Admin profiles
        if (_currentUser.Role == "Manager" && user.Role == UserRole.Admin)
            throw new InvalidOperationException("Managers cannot delete Admin profiles.");

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

        await _audit.LogAsync("PasswordChange", "User", user.Id.ToString(), new
        {
            Username = user.Username
        });
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

        await _audit.LogAsync(user.IsActive ? "UserActivated" : "UserDeactivated", "User", user.Id.ToString(), new
        {
            Username = user.Username, FullName = user.FullName, IsActive = user.IsActive
        });

        _ = _notificationService.NotifyDataChangedAsync(orgId, NotificationConstants.UserChanged);
    }
}
