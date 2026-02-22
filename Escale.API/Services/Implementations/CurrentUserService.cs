using System.Security.Claims;
using Escale.API.Services.Interfaces;

namespace Escale.API.Services.Implementations;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var claim = User?.FindFirst("UserId")?.Value;
            return claim != null && Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public Guid? OrganizationId
    {
        get
        {
            var claim = User?.FindFirst("OrganizationId")?.Value;
            return claim != null && Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value;
    public string? Username => User?.FindFirst(ClaimTypes.Name)?.Value;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
