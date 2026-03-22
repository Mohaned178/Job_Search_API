using JobMarket.Domain.Entities;

namespace JobMarket.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    (string Token, DateTime Expiry) GenerateRefreshToken();
}
