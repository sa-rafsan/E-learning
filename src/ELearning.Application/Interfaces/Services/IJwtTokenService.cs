using ELearning.Domain.Entities;

namespace ELearning.Application.Interfaces.Services;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(User user);

    (string RawToken, string TokenHash) GenerateRefreshToken();

    string HashRefreshToken(string rawToken);
}

