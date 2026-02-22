using System.Security.Claims;

namespace Escale.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst("UserId")?.Value;
        return claim != null ? Guid.Parse(claim) : throw new UnauthorizedAccessException("UserId claim not found");
    }

    public static Guid GetOrganizationId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst("OrganizationId")?.Value;
        return claim != null ? Guid.Parse(claim) : throw new UnauthorizedAccessException("OrganizationId claim not found");
    }

    public static string GetRole(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Role)?.Value ?? throw new UnauthorizedAccessException("Role claim not found");
    }
}
