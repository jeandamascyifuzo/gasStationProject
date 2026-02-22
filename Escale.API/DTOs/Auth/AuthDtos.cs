namespace Escale.API.DTOs.Auth;

public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public class LoginResponseDto
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public UserInfoDto? User { get; set; }
}

public class RegisterRequestDto
{
    public string OrganizationName { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string AdminFullName { get; set; } = string.Empty;
    public string? AdminEmail { get; set; }
    public string? Phone { get; set; }
    public string? TIN { get; set; }
    public string? Address { get; set; }
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<StationInfoDto> AssignedStations { get; set; } = new();
}

public class StationInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Address { get; set; }
}
