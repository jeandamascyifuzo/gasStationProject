namespace Escale.API.Services.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? OrganizationId { get; }
    string? Role { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
    bool IsSuperAdmin { get; }
}
