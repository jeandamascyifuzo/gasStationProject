using Escale.API.Domain.Entities;

namespace Escale.API.Services.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
