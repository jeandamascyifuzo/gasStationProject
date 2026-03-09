using AutoMapper;
using Escale.API.Data.Repositories;
using Escale.API.Domain.Constants;
using Escale.API.Domain.Entities;
using Escale.API.Domain.Enums;
using Escale.API.DTOs.Auth;
using Escale.API.DTOs.Users;
using Escale.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Escale.API.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, IMapper mapper,
        IConfiguration configuration, ILogger<AuthService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _mapper = mapper;
        _configuration = configuration;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Single query: get user with stations in one round-trip
        var user = await _unitOfWork.Users.Query()
            .Include(u => u.UserStations).ThenInclude(us => us.Station)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        var dbTime = sw.ElapsedMilliseconds;

        if (user == null)
            return new LoginResponseDto { Success = false, Message = "Invalid username or password" };

        var bcryptResult = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        var bcryptTime = sw.ElapsedMilliseconds - dbTime;

        _logger.LogInformation("[Login] DB query: {DbTime}ms, BCrypt verify: {BcryptTime}ms", dbTime, bcryptTime);

        if (!bcryptResult)
        {
            await LogFailedLoginAsync(user, "Invalid password");
            return new LoginResponseDto { Success = false, Message = "Invalid username or password" };
        }

        if (!user.IsActive)
        {
            await LogFailedLoginAsync(user, "Account disabled");
            return new LoginResponseDto { Success = false, Message = "Account is disabled" };
        }

        // Check if organization is active (skip for SuperAdmin)
        Organization? org = null;
        if (user.Role != UserRole.SuperAdmin)
        {
            org = await _unitOfWork.Organizations.GetByIdAsync(user.OrganizationId);
            if (org == null || !org.IsActive)
                return new LoginResponseDto { Success = false, Message = "Unable to sign in. Please contact your administrator for assistance." };
        }

        // Filter to active stations only
        user.UserStations = user.UserStations.Where(us => us.Station.IsActive).ToList();

        if (user.UserStations.Count == 0 && (user.Role == UserRole.Cashier || user.Role == UserRole.Supervisor))
            return new LoginResponseDto { Success = false, Message = "No active station is assigned to your account. Please contact your administrator for assistance." };

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"]!);

        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
            CreatedAt = DateTime.UtcNow
        });

        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("[Login] Total: {TotalMs}ms (DB: {DbMs}ms, BCrypt: {BcryptMs}ms, rest: {RestMs}ms)",
            sw.ElapsedMilliseconds, dbTime, bcryptTime, sw.ElapsedMilliseconds - dbTime - bcryptTime);

        // Log login to audit trail
        var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        _unitOfWork.Context.AuditLogs.Add(new AuditLog
        {
            OrganizationId = user.Role != UserRole.SuperAdmin ? user.OrganizationId : null,
            UserId = user.Id,
            UserName = user.FullName,
            Action = "Login",
            EntityType = "User",
            EntityId = user.Id.ToString(),
            Details = System.Text.Json.JsonSerializer.Serialize(new
            {
                Username = user.Username,
                Role = user.Role.ToString(),
                StationCount = user.UserStations.Count
            }),
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        var userInfo = _mapper.Map<UserInfoDto>(user);
        if (org != null)
        {
            userInfo.OrganizationName = org.Name;
            userInfo.LogoUrl = org.LogoUrl;
        }

        return new LoginResponseDto
        {
            Success = true,
            Token = accessToken,
            RefreshToken = refreshToken,
            Message = "Login successful",
            User = userInfo
        };
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingOrg = await _unitOfWork.Organizations.ExistsAsync(o => o.Name == request.OrganizationName);
        if (existingOrg)
            throw new InvalidOperationException("Organization name already exists");

        var orgId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var organization = new Organization
        {
            Id = orgId,
            Name = request.OrganizationName,
            Slug = request.OrganizationName.ToLower().Replace(" ", "-"),
            TIN = request.TIN,
            Address = request.Address,
            Phone = request.Phone,
            IsActive = true,
            CreatedAt = now
        };
        await _unitOfWork.Organizations.AddAsync(organization);

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            Username = request.AdminUsername,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword),
            FullName = request.AdminFullName,
            Email = request.AdminEmail,
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = now
        };
        await _unitOfWork.Users.AddAsync(adminUser);

        // Default fuel types
        var fuelTypes = new[]
        {
            new FuelType { Id = Guid.NewGuid(), OrganizationId = orgId, Name = "Petrol 95", CurrentPrice = 1450m, IsActive = true, CreatedAt = now },
            new FuelType { Id = Guid.NewGuid(), OrganizationId = orgId, Name = "Petrol 98", CurrentPrice = 1550m, IsActive = true, CreatedAt = now },
            new FuelType { Id = Guid.NewGuid(), OrganizationId = orgId, Name = "Diesel", CurrentPrice = 1380m, IsActive = true, CreatedAt = now },
            new FuelType { Id = Guid.NewGuid(), OrganizationId = orgId, Name = "Kerosene", CurrentPrice = 1200m, IsActive = true, CreatedAt = now }
        };
        foreach (var ft in fuelTypes)
        {
            await _unitOfWork.FuelTypes.AddAsync(ft);
            await _unitOfWork.FuelPrices.AddAsync(new FuelPrice
            {
                Id = Guid.NewGuid(), FuelTypeId = ft.Id, Price = ft.CurrentPrice,
                EffectiveFrom = now, CreatedAt = now
            });
        }

        // Default settings
        await _unitOfWork.OrganizationSettings.AddAsync(new OrganizationSettings
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            CompanyName = request.OrganizationName,
            TaxRate = BusinessRules.VATRate,
            Currency = BusinessRules.DefaultCurrency,
            ReceiptHeader = $"{request.OrganizationName}\nKigali, Rwanda",
            ReceiptFooter = BusinessRules.DefaultReceiptFooter,
            EBMEnabled = false,
            MinimumSaleAmount = BusinessRules.DefaultMinimumSaleAmount,
            MaximumSaleAmount = BusinessRules.DefaultMaximumSaleAmount,
            LowStockThreshold = 0.20m,
            CriticalStockThreshold = 0.10m,
            CreatedAt = now
        });

        await _unitOfWork.SaveChangesAsync();

        // Auto-login
        var token = _tokenService.GenerateAccessToken(adminUser);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"]!);

        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = adminUser.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        return new LoginResponseDto
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken,
            Message = "Registration successful",
            User = new UserInfoDto
            {
                Id = adminUser.Id,
                Username = adminUser.Username,
                FullName = adminUser.FullName,
                Role = adminUser.Role.ToString(),
                AssignedStations = new List<StationInfoDto>()
            }
        };
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var existingToken = await _unitOfWork.RefreshTokens.Query()
            .Include(rt => rt.User).ThenInclude(u => u.UserStations).ThenInclude(us => us.Station)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow);

        if (existingToken == null)
            return new LoginResponseDto { Success = false, Message = "Invalid or expired refresh token" };

        // Check if organization is still active
        if (existingToken.User.Role != UserRole.SuperAdmin)
        {
            var org = await _unitOfWork.Organizations.GetByIdAsync(existingToken.User.OrganizationId);
            if (org == null || !org.IsActive)
                return new LoginResponseDto { Success = false, Message = "Unable to sign in. Please contact your administrator for assistance." };
        }

        // Revoke old token
        existingToken.IsRevoked = true;
        existingToken.RevokedAt = DateTime.UtcNow;

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(existingToken.User);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var refreshDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"]!);

        existingToken.ReplacedByToken = newRefreshToken;
        _unitOfWork.RefreshTokens.Update(existingToken);

        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = existingToken.UserId,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        return new LoginResponseDto
        {
            Success = true,
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            Message = "Token refreshed",
            User = _mapper.Map<UserInfoDto>(existingToken.User)
        };
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var token = await _unitOfWork.RefreshTokens.Query()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

        if (token == null)
            throw new KeyNotFoundException("Token not found");

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        _unitOfWork.RefreshTokens.Update(token);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ProfileResponseDto> GetProfileAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        return new ProfileResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString()
        };
    }

    public async Task<ProfileResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Phone = request.Phone;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return new ProfileResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString()
        };
    }

    public async Task ChangeOwnPasswordAsync(Guid userId, ChangePasswordRequestDto request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("Current password is incorrect");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task LogFailedLoginAsync(User user, string reason)
    {
        try
        {
            var ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            _unitOfWork.Context.AuditLogs.Add(new AuditLog
            {
                OrganizationId = user.OrganizationId != Guid.Empty ? user.OrganizationId : null,
                UserId = user.Id,
                UserName = user.FullName,
                Action = "LoginFailed",
                EntityType = "User",
                EntityId = user.Id.ToString(),
                Details = System.Text.Json.JsonSerializer.Serialize(new { Username = user.Username, Reason = reason }),
                IpAddress = ip,
                Timestamp = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();
        }
        catch { /* Don't fail login flow for audit issues */ }
    }
}
